namespace Jarchive;

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

public class Store
{
    private readonly IMemoryCache _cache;
    private List<string> _admins;
    private ConcurrentDictionary<string, Metadata> _meta;
    private BlockingCollection<Metadata> _dirty;
    private string path;

    public Store(
        IHostEnvironment env,
        AppSettings settings,
        IMemoryCache cache
    )
    {
        _cache = cache;
        _meta = new();
        _admins = new();
        _dirty = new();
        Task.Run(() => SaveDirtyThread());
        path = Path.Combine(env.ContentRootPath, settings.RootFolder);
        Reload();
    }

    public void Reload()
    {
        Directory.CreateDirectory(path);

        _meta.Clear();
        string[] files = Directory.GetFiles(path, AppConstants.MetadataFilename, SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string key = Path.GetDirectoryName(file)!.Split(Path.DirectorySeparatorChar).Last();
            Metadata meta = JsonSerializer.Deserialize<Metadata>(System.IO.File.ReadAllText(file)) ?? new();
            meta.Key = key;
            _meta.TryAdd(key, meta);
        }

        _admins.Clear();
        string rootfile = Path.Combine(path, AppConstants.RootMetaFilename);
        if (System.IO.File.Exists(rootfile).Equals(false))
        {
            System.IO.File.WriteAllText(
                rootfile,
                JsonSerializer.Serialize(new Metadata())
            );
        }

        Metadata root = JsonSerializer.Deserialize<Metadata>(
            System.IO.File.ReadAllText(rootfile)
        ) ?? new();

        _admins.AddRange(root.Owners);
    }

    public Metadata Create(string owner, string name = "New Folder")
    {
        Metadata meta = new();
        meta.Key = Guid.NewGuid().ToString("n");
        meta.Name = name;
        meta.CreationTime = DateTimeOffset.UtcNow;
        meta.Owners.Add(owner);
        _meta.TryAdd(meta.Key, meta);
        SaveChanges(meta);
        return meta;
    }

    public Metadata Retrieve(string key)
    {
        if (_meta.TryGetValue(key, out Metadata? meta))
            return meta;

        string metafile = Path.Combine(path, key, AppConstants.MetadataFilename);
        if (System.IO.File.Exists(metafile) == false)
            return new();

        meta = System.Text.Json.JsonSerializer.Deserialize<Metadata>(
            System.IO.File.ReadAllText(metafile)
        ) ?? new();

        _meta.TryAdd(key, meta);

        return meta;
    }

    public void Update(Metadata changes)
    {
        Metadata orig = Retrieve(changes.Key);
        Metadata changed = orig.Clone();
        changed.Name = changes.Name;
        changed.Scope = changes.Scope;
        changed.Description = changes.Description;

        Update(changed, orig);
    }

    private void Update(Metadata meta, Metadata orig)
    {
        if (_meta.TryUpdate(meta.Key, meta, orig))
        {
            _dirty.Add(meta);
            Uncache(meta.Key);
        }
        else
            throw new Exception(Message.UpdateHadCollision);
    }

    public void Delete(string key)
    {
        Directory.Delete(
            Path.Combine(path, key),
            true
        );

        _meta.TryRemove(key, out _);

        Uncache(key);
    }

    public void Uncache(string key)
    {
        _cache.Remove(key);
    }

    public Folder[] List(string actor, string filter)
    {
        var list = actor == string.Empty
            ? _meta.Values.Where(f => f.IsPublic)
            : _admins.Contains(actor)
                ? _meta.Values
                : _meta.Values.Where(f =>
                    f.IsPublic ||
                    f.IsInternal ||
                    f.Owners.Contains(actor) ||
                    f.Writers.Contains(actor) ||
                    f.Readers.Contains(actor)
                )
        ;

        // if (filter == "all" && _admins.Contains(actor))
        //     list = _meta.Values;

        return list.Select(o =>
            new Folder
            {
                Key = o.Key,
                Name = o.Name,
                Description = o.Description,
                Scope = o.Scope
            }
        ).ToArray();
    }

    public Folder? Resolve(string key, string url)
    {
        if (_cache.TryGetValue<Folder>(key, out Folder? folder))
            return folder;

        Metadata meta = Retrieve(key);

        folder = new();
        folder.Key = key;
        folder.Name = meta.Name;
        folder.Description = meta.Description;
        folder.Scope = meta.Scope;

        var filenames = System.IO.Directory.GetFiles(
            Path.Combine(path, key)
        ).Where(f => !f.Contains(AppConstants.MetadataFilename));

        foreach (string filename in filenames)
        {
            System.IO.FileInfo f = new(filename);
            folder.Files.Add(new File(
                f.Name,
                f.Length,
                f.CreationTimeUtc,
                string.Format(url, key, meta.Name, f.Name)
            ));
        }

        _cache.Set(key, folder);

        return folder;
    }

    public Folder? Resolve(string key, string subject, string url)
    {
        Metadata meta = Retrieve(key);
        Folder folder = Resolve(key, url) ?? new();

        folder!.Role = _admins.Contains(subject)
            ? FolderRole.Admin
            : meta.ResolveRole(subject)
        ;

        return folder;
    }

    public bool IsAdmin(string subject)
    {
        return _admins.Contains(subject);
    }

    private void SaveDirtyThread()
    {
        while (!_dirty.IsCompleted)
        {
            Metadata? meta = null;
            try
            {
                meta = _dirty.Take();
            }
            catch (InvalidOperationException) { }

            if (meta is null)
                continue;

            SaveChanges(meta);
        }
    }

    private void SaveChanges(Metadata meta)
    {
        System.IO.Directory.CreateDirectory(
            Path.Combine(path, meta.Key)
        );

        System.IO.File.WriteAllText(
            Path.Combine(path, meta.Key, AppConstants.MetadataFilename),
            JsonSerializer.Serialize(meta)
        );
    }

    public async Task SaveFile(string key, string filename, Stream stream)
    {
        using var writer = System.IO.File.OpenWrite(
            ResolvePath(key, filename)
        );
        await stream.CopyToAsync(writer);

        Uncache(key);
    }

    public void DeleteFile(string key, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Equals(AppConstants.MetadataFilename))
            return;

        System.IO.File.Delete(
            ResolvePath(key, name)
        );

        Uncache(key);
    }

    public void SetRole(string key, string subject, FolderRole role)
    {
        Metadata orig = Retrieve(key);
        Metadata changed = orig.Clone();

        switch (role)
        {
            case FolderRole.Owner:
                if (!changed.IsOwner(subject))
                    changed.Owners.Add(subject);
                break;

            case FolderRole.Writer:
                if (!changed.Writers.Contains(subject))
                    changed.Writers.Add(subject);
                break;

            case FolderRole.Reader:
                if (!changed.Readers.Contains(subject))
                    changed.Readers.Add(subject);
                break;
        }

        Update(changed, orig);
    }

    public void ClearRoles(string key, string subject)
    {
        Metadata orig = Retrieve(key);
        Metadata changed = orig.Clone();
        changed.Owners.Clear();
        changed.Writers.Clear();
        changed.Readers.Clear();
        changed.Owners.Add(subject);
        Update(changed, orig);
    }

    public string ResolvePath(string folder, string filename)
    {
        return Path.Combine(
            path,
            folder,
            filename.SanitizeFilename()
        );
    }

    public void RenameFile(string key, string name, string change)
    {
        string src = ResolvePath(key, name);
        string dst = ResolvePath(key, change);

        System.IO.File.Move(src, dst);

        Uncache(key);
    }

    public bool FileExists(string key, string name)
    {
        return System.IO.File.Exists(
            ResolvePath(key, name)
        );
    }
}
