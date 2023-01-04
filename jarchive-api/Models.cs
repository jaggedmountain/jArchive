using System.ComponentModel.DataAnnotations;

namespace Jarchive;

public class Metadata
{
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTimeOffset CreationTime { get; set; }
    public FolderScope Scope { get; set; }
    public bool IsPublic => Scope == FolderScope.Public;
    public bool IsInternal => Scope == FolderScope.Internal;
    public List<string> Owners { get; set; } = new();
    public List<string> Readers { get; set; } = new();
    public List<string> Writers { get; set; } = new();

    public FolderRole ResolveRole(string subject)
    {
        if (IsOwner(subject))
            return FolderRole.Owner;

        if (IsWriter(subject))
            return FolderRole.Writer;

        if (IsReader(subject))
            return FolderRole.Reader;

        return FolderRole.None;
    }

    public bool IsReader(string subject, bool sudo = false)
    {
        return subject == string.Empty
            ? IsPublic
            : IsPublic || sudo ||
            Scope == FolderScope.Internal ||
            Owners.Contains(subject) ||
            Writers.Contains(subject) ||
            Readers.Contains(subject)
        ;
    }

    public bool IsWriter(string subject, bool sudo = false)
    {
        return
            sudo ||
            Owners.Contains(subject) ||
            Writers.Contains(subject)
        ;
    }

    public bool IsOwner(string subject, bool sudo = false)
    {
        return sudo || Owners.Contains(subject);
    }

    public Metadata Clone()
    {
        return new()
        {
            Key = Key,
            Name = Name,
            Description = Description,
            Scope = Scope,
            CreationTime = CreationTime,
            Owners = Owners,
            Writers = Writers,
            Readers = Readers
        };
    }
}

public enum FolderScope
{
    Specified,
    Internal,
    Public
}

public enum FolderRole
{
    None,
    Reader,
    Writer,
    Owner,
    Admin
}

public class Folder
{
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public FolderScope Scope { get; set; }
    public FolderRole Role { get; set; }
    public List<File> Files { get; set; } = new();
}

public class NewFolder
{
    [MaxLength(255)]
    public string Name { get; set; } = "New Folder";
}

public class ChangedFile
{
    [MaxLength(64)]
    public string Key { get; set; } = default!;

    [MaxLength(255)]
    public string OldName { get; set; } = "";

    [MaxLength(255)]
    public string NewName { get; set; } = "";
}

public class ChangedFolder
{
    [MaxLength(64)]
    public string Key { get; set; } = default!;
    [MaxLength(255)]
    public string Name { get; set; } = default!;
    [MaxLength(1024)]
    public string? Description { get; set; } = default!;
    public FolderScope Scope { get; set; }
}

public class Invitation
{
    [MaxLength(64)]
    public string Key { get; set; } = default!;
    public FolderRole Role { get; set; }
    public int ExpirationMinutes { get; set; } = 30;
    public bool MultipleUse { get; set; }
    [MaxLength(64)]
    public string? Token { get; set; }
}

public record class File(
    string Name,
    long Length,
    DateTimeOffset CreationTime,
    string Url
);

public record class User(
    string Id,
    string Name,
    bool IsAdmin
);
