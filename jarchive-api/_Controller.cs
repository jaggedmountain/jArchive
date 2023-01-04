using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Jarchive;

public class _Controller: ControllerBase
{
    public _Controller(
        ILogger logger
    )
    {
        Logger = logger;
        _jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
    }

    public ILogger Logger { get; }

    private readonly JsonSerializerOptions _jsonOptions;

    protected IResult JsonError(string msg, object model = default!)
    {
        string json = model != null
            ? JsonSerializer.Serialize(model, _jsonOptions)
            : ""
        ;

        Logger.LogError("{message}\nuser:{subject} authed:{auth}\n{path}{query}\n{json}",
            msg,
            User.Subject(),
            User.Identity?.IsAuthenticated ?? false,
            Request.Path,
            Request.QueryString,
            json
        );

        return Results.BadRequest(new {message = msg});
    }

}
