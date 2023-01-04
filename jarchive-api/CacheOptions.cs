namespace Jarchive;

public class CacheOptions
{
    public string Key { get; set; } = "jarchive";
    public string RedisUrl { get; set; } = default!;
    public string DataProtectionFolder { get; set; } = ".dpk";
    public int CacheExpirationSeconds { get; set; } = 300;
}
