using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using Jarchive;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, Func<CacheOptions> configure)
        {
            var options = configure();

            services.AddMemoryCache();

            if (System.String.IsNullOrWhiteSpace(options.RedisUrl))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(opt => {
                    opt.Configuration = options.RedisUrl;
                    opt.InstanceName = options.Key;
                });
            }

            return services;
        }

        public static IDataProtectionBuilder PersistKeys(this IDataProtectionBuilder builder, Func<CacheOptions> configure)
        {
            var options = configure();

            if (string.IsNullOrWhiteSpace(options.RedisUrl))
            {
                Directory.CreateDirectory(options.DataProtectionFolder);

                builder.PersistKeysToFileSystem(
                    new DirectoryInfo(options.DataProtectionFolder)
                );
            }
            else
            {
                builder.PersistKeysToStackExchangeRedis(
                    ConnectionMultiplexer.Connect(options.RedisUrl),
                    $"{options.Key}-dpk"
                );
            }

            return builder;
        }
    }
}
