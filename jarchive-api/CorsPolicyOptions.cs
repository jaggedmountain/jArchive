using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Jarchive;

public class CorsPolicyOptions
{
    public string Name { get; set; } = "default";
    public string[] Origins { get; set; } = new string[] { };
    public string[] Methods { get; set; } = new string[] { };
    public string[] Headers { get; set; } = new string[] { };
    public bool AllowCredentials { get; set; }
    public bool AllowWildcardSubdomains { get; set; } = true;
    public int PreflightMaxAgeMinutes { get; set; } = 10;

    public CorsPolicy Build()
    {
        CorsPolicyBuilder policy = new CorsPolicyBuilder();

        var origins = Origins.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (origins.Any())
        {
            if (origins.First() == "*") policy.AllowAnyOrigin(); else policy.WithOrigins(origins);
            if (AllowCredentials && origins.First() != "*") policy.AllowCredentials(); else policy.DisallowCredentials();
        }

        var methods = Methods.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (methods.Any())
        {
            if (methods.First() == "*") policy.AllowAnyMethod(); else policy.WithMethods(methods);
        }

        var headers = Headers.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (headers.Any())
        {
            if (headers.First() == "*") policy.AllowAnyHeader(); else policy.WithHeaders(headers);
        }

        if (AllowWildcardSubdomains)
            policy.SetIsOriginAllowedToAllowWildcardSubdomains();

        policy.SetPreflightMaxAge(new TimeSpan(0, PreflightMaxAgeMinutes, 0));

        return policy.Build();
    }
}
