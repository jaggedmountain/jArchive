using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Jarchive;

[Authorize]
[ApiController]
public class FileController : _Controller
{
    public FileController(
        ILogger<FileController> logger,
        AppSettings settings,
        Store store
    ) : base(logger)
    {
        Settings = settings;
        Store = store;
    }

    AppSettings Settings { get; }
    Store Store { get; }

    /// <summary>
    /// Upload a file.
    /// </summary>
    /// <remarks>
    /// Expects mime-multipart body with single file
    /// </remarks>
    [Authorize]
    [HttpPost("api/file/upload/{folder}")]
    [DisableFormValueModelBinding]
    [DisableRequestSizeLimit]
    public async Task<IResult> UploadFile(CancellationToken ct = default)
    {
        FormOptions options = new();

        long maxsize = Settings.MaxFileSize.ToSizeBi();
        if (maxsize > 0)
            options.MultipartBodyLengthLimit = maxsize;

        long length = Request.Headers.ContentLength ?? 0;
        if (length > options.MultipartBodyLengthLimit)
            return JsonError(Message.FilesizeExceedsLimit);

        string key = Request.Path.Value!.Split('/').Last();
        var metadata = Store.Retrieve(key);

        if (metadata.IsWriter(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        try
        {
            await ProcessUpload(Request, options, ct, filename =>
                {
                    string cleanname = filename.SanitizeFilename();
                    if (cleanname.Equals(AppConstants.MetadataFilename))
                        throw new Exception(Message.UploadFilenameInvalid);

                    Logger.LogInformation($"Uploading {key}/{cleanname} length:{length} subject:{User.Subject()}");

                    return System.IO.File.Create(
                        Store.ResolvePath(key, cleanname)
                    );
                }
            );
        }
        catch (Exception ex)
        {
            return JsonError(ex.Message);
        }

        Store.Uncache(key);
        return TypedResults.Ok();
    }

    [HttpPut("api/file/rename")]
    [Authorize]
    public IResult Update(ChangedFile model)
    {
        var metadata = Store.Retrieve(model.Key);

        if (model.OldName.Equals(AppConstants.MetadataFilename))
            return TypedResults.Forbid();

        if (metadata.IsWriter(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        if (
            Store.FileExists(model.Key, model.OldName) == false ||
            Store.FileExists(model.Key, model.NewName) == true
        )
            return JsonError(Message.FileRenameFailed, model);

        Store.RenameFile(model.Key, model.OldName, model.NewName);
        return TypedResults.Ok();
    }

    [HttpDelete("api/file/{folder}/{name}")]
    [Authorize]
    public IResult Delete(string folder, string name)
    {
        Metadata meta = Store.Retrieve(folder);

        if (name.Equals(AppConstants.MetadataFilename))
            return TypedResults.Forbid();

        if (meta.IsOwner(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        Store.DeleteFile(folder, name);
        return TypedResults.Ok();
    }

    [HttpGet("get/{key}/{folder}/{name}")]
    [Authorize(AppConstants.ReaderPolicy)]
    public IResult DownloadProtectedFile(string key, string folder, string name)
    {
        Metadata metadata = Store.Retrieve(key);
        if (metadata.IsReader(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        return DownloadFile(key, name);
    }

    [HttpGet("public/{key}/{folder}/{name}")]
    [AllowAnonymous]
    public IResult DownloadPublicFile(string key, string folder, string name)
    {
        Metadata meta = Store.Retrieve(key);
        if (meta.IsPublic == false)
            return TypedResults.Forbid();

        return DownloadFile(key, name);
    }

    private IResult DownloadFile(string key, string name)
    {
        string path = Store.ResolvePath(key, name);

        if (System.IO.File.Exists(path) == false)
            return TypedResults.NotFound();

        if (name.Equals(AppConstants.MetadataFilename))
            return TypedResults.Forbid();

        new FileExtensionContentTypeProvider().TryGetContentType(name, out string? contentType);

        return Results.File(
            path,
            contentType ?? "application/octet-stream"
        );
    }

    [HttpGet("api/reader")]
    [Authorize]
    public async Task<IResult> GetReaderCookie()
    {
        Claim[] claims = new Claim[] {
            new Claim(AppConstants.SubjectClaimName, User.Subject()),
            new Claim(AppConstants.NameClaimName, User.Name())
        };

        await HttpContext.SignInAsync(
                AppConstants.CookieScheme,
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, AppConstants.CookieScheme)
                ),
                new AuthenticationProperties()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                }
            );

        return TypedResults.Ok();
    }

    [HttpPost("api/reader/signout")]
    [Authorize(AppConstants.ReaderPolicy)]
    public async Task<IResult> InvalidateReaderCookie()
    {
        await HttpContext.SignOutAsync(AppConstants.CookieScheme);
        return TypedResults.Ok();
    }

    private async Task ProcessUpload(
        HttpRequest request,
        FormOptions options,
        CancellationToken ct,
        Func<string, Stream> getDestinationStream
    )
    {
        if (request.ContentType is null)
            throw new Exception(Message.RequestNotMultipart);

        if (request.ContentType!.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) < 0)
            throw new Exception(Message.RequestNotMultipart);

        string? boundary = HeaderUtilities.RemoveQuotes(
            MediaTypeHeaderValue.Parse(request.ContentType!).Boundary.Value
        ).Value;

        if (string.IsNullOrWhiteSpace(boundary))
            throw new Exception(Message.RequestNotMultipart);

        MultipartReader reader = new MultipartReader(boundary, request.Body);
        MultipartSection? section = await reader.ReadNextSectionAsync();
        while (section is MultipartSection)
        {
            if (ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out ContentDispositionHeaderValue? contentDisposition)
            )
            {
                if (
                    contentDisposition.DispositionType.Equals("form-data")
                    &&
                    string.IsNullOrEmpty(contentDisposition.FileName.Value).Equals(false)
                )
                {
                    string filename = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value!;

                    Stream dest = getDestinationStream.Invoke(filename);
                    try
                    {
                        if (Settings.UploadUsingSave)
                            await Save(section.Body, dest, Settings.UploadBufferSize, filename, ct);
                        else
                            await CopyStreams(section.Body, dest, Settings.UploadBufferSize, filename, ct);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{Message.UploadFailed} {filename}", ex);
                    }
                    finally
                    {
                        await dest.FlushAsync();
                        dest.Dispose();
                    }

                    // only processing one file per request, so bail
                    return;
                }
            }

            section = await reader.ReadNextSectionAsync();
        }

        throw new Exception(Message.UploadMissingFilename);
    }

    private async Task CopyStreams(Stream source, Stream dest, int buffersize, string path, CancellationToken ct)
    {
        Logger.LogInformation($"Upload starting: {path}");
        var started = DateTimeOffset.UtcNow;
        await source.CopyToAsync(dest, buffersize, ct);
        long totalBytes = dest.Length;
        long duration = (long)DateTimeOffset.UtcNow.Subtract(started).TotalSeconds;
        long rate = totalBytes / duration;
        Logger.LogInformation($"Upload complete: {totalBytes}b {duration}s {rate}b/s {path}");
    }

    private async Task Save(Stream source, Stream dest, int buffersize, string path, CancellationToken ct)
    {
        byte[] buffer = new byte[buffersize];
        int bytes = 0;
        long totalBlocks = 0;
        long totalBytes = 0;
        long duration = 0;
        long rate = 0;

        Logger.LogInformation($"Upload starting: {path}");
        var started = DateTimeOffset.UtcNow;

        do
        {
            bytes = await source.ReadAsync(buffer, 0, buffer.Length, ct);
            await dest.WriteAsync(buffer, 0, bytes, ct);
            totalBlocks += 1;
            totalBytes += bytes;
            if (totalBlocks % 1024 == 0)
            {
                duration = (long)DateTimeOffset.UtcNow.Subtract(started).TotalSeconds;
                rate = totalBytes / duration;
                Logger.LogDebug($"Upload pending: {totalBytes}b {duration}s {rate}b/s {path}");
            }
        } while (bytes > 0);

        duration = (long)DateTimeOffset.UtcNow.Subtract(started).TotalSeconds;
        rate = totalBytes / duration;
        Logger.LogInformation($"Upload complete: {totalBytes}b {duration}s {rate}b/s {path}");
    }
}
