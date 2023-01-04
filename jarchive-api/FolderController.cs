using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Jarchive.Controllers;

[ApiController]
[Authorize]
public class FolderController : _Controller
{
    IDistributedCache Cache { get; }
    Store Store { get; }

    public FolderController(
        ILogger<FolderController> logger,
        IDistributedCache cache,
        Store store
    ) : base(logger)
    {
        Cache = cache;
        Store = store;
    }

    [HttpGet("api/folders")]
    [AllowAnonymous]
    public IResult GetFolders([FromQuery] string? filter)
    {
        return TypedResults.Ok(
            Store.List(User.Subject(), filter ?? "")
        );
    }

    [HttpGet("api/folder/{key}")]
    [AllowAnonymous]
    public IResult Get([FromRoute] string key)
    {
        Metadata meta = Store.Retrieve(key);

        if (meta.IsReader(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        return TypedResults.Ok(
            Store.Resolve(
                key,
                User.Subject(),
                Request.FileLinkUri(meta.IsPublic)
            )
        );
    }

    [HttpPost("api/folder")]
    [Authorize]
    public IResult Create(NewFolder model)
    {
        var meta = Store.Create(User.Subject(), model.Name);
        return TypedResults.Created(
            Url.Action(nameof(Get), new { key = meta.Key })!,
            Store.Resolve(
                meta.Key,
                User.Subject(),
                Request.FileLinkUri(meta.IsPublic)
            )
        );
    }

    [HttpPut("api/folder")]
    [Authorize]
    public IResult Update([FromBody] ChangedFolder model)
    {
        Metadata meta = Store.Retrieve(model.Key);
        if (meta.IsOwner(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        Metadata changes = meta.Clone();
        changes.Name = model.Name;
        changes.Scope = model.Scope;
        changes.Description = model.Description ?? "";

        try
        {
            Store.Update(changes);
        }
        catch (Exception ex)
        {
            return JsonError(ex.Message, model);
        }
        return TypedResults.Ok();
    }

    [HttpDelete("api/folder/{key}")]
    [Authorize]
    public IResult Delete(string key)
    {
        Metadata meta = Store.Retrieve(key);
        if (meta.IsOwner(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        Store.Delete(key);
        return TypedResults.Ok();
    }

    [HttpPost("api/folder/invite")]
    [Authorize]
    public async Task<IResult> Invite(Invitation invitation)
    {
        Metadata meta = Store.Retrieve(invitation.Key);
        if (meta.IsOwner(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        invitation.Token = new Random().NextInt64().ToString("x");

        DistributedCacheEntryOptions opts = new()
        {
            AbsoluteExpirationRelativeToNow = new TimeSpan(
                0,
                invitation.ExpirationMinutes,
                0
            )
        };

        await Cache.SetStringAsync(
            AppConstants.ShareCodeCachePrefix + invitation.Token,
            JsonSerializer.Serialize(invitation),
            opts
        );

        return TypedResults.Ok(invitation);
    }

    [HttpPut("api/folder/redeem/{code}")]
    [Authorize]
    public async Task<IResult> Redeem(string code)
    {
        Invitation? invitation = null;

        string? value = await Cache.GetStringAsync(AppConstants.ShareCodeCachePrefix + code);

        if (value == null)
            return JsonError(Message.InvalidAccessToken);

        try
        {
            invitation = JsonSerializer.Deserialize<Invitation>(value);
        }
        catch {}

        if (invitation == null)
            return JsonError(Message.InvalidAccessToken);

        if (invitation.MultipleUse.Equals(false))
            await Cache.RemoveAsync(AppConstants.ShareCodeCachePrefix + code);

        Store.SetRole(
            invitation!.Key,
            User.Subject(),
            invitation.Role
        );

        var meta = Store.Retrieve(invitation.Key);

        return TypedResults.Ok(
            Store.Resolve(
                invitation.Key,
                User.Subject(),
                Request.FileLinkUri(meta.IsPublic)
            )
        );
    }

    [HttpPost("api/folder/reset/{key}")]
    [Authorize]
    public IResult Reset(string key)
    {
        Metadata meta = Store.Retrieve(key);
        if (meta.IsOwner(User.Subject(), Store.IsAdmin(User.Subject())).Equals(false))
            return TypedResults.Forbid();

        Store.ClearRoles(key, User.Subject());

        return TypedResults.Ok(
            Store.Resolve(
                key,
                User.Subject(),
                Request.FileLinkUri(meta.IsPublic)
            )
        );
    }

    [HttpPost("api/admin/reload")]
    [Authorize]
    public IResult Reload()
    {
        if (Store.IsAdmin(User.Subject()).Equals(false))
            return TypedResults.Forbid();

        Store.Reload();

        return TypedResults.Ok();
    }
}
