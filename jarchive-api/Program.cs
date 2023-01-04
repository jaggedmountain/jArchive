using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

namespace Jarchive;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        AppSettings Settings = builder.Configuration.Get<AppSettings>() ?? new AppSettings();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters
                    .Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            })
        ;
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            );
        });

        builder.Services.AddCors(
            opt => opt.AddPolicy(
                Settings.Cors.Name,
                Settings.Cors.Build()
            )
        );

        builder.Services.AddDataProtection()
            .SetApplicationName(AppConstants.DataProtectionPurpose)
            .PersistKeys(() => Settings.Cache)
        ;

        builder.Services.AddCache(() => Settings.Cache);
        builder.Services.AddSingleton(_ => Settings);
        builder.Services.AddSingleton<Store>();

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                options => builder.Configuration.Bind(AppConstants.OpenIdSettingsKey, options)
            )
            .AddCookie(AppConstants.CookieScheme, opt =>
            {
                opt.Cookie.Name = AppConstants.CookieScheme;
                opt.LoginPath = "/reader";
            })
        ;

        builder.Services.AddAuthorization(_ =>
            {
                _.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme
                    ).Build()
                ;

                _.AddPolicy(AppConstants.ReaderPolicy, new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(AppConstants.CookieScheme)
                    .Build()
                );

            })
        ;

        var app = builder.Build();
        app.UsePathBase(Settings.PathBase);
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors(Settings.Cors.Name);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        app.Run();
    }
}
