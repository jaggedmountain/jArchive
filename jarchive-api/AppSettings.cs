namespace Jarchive;

public class AppSettings
{
    public string PathBase { get; set; } = default!;
    public string RootFolder { get; set; } = "_jarchive";
    public long MaxFileSize { get; set; } = 134217728;
    public CorsPolicyOptions Cors { get; set; } = new();
    public CacheOptions Cache { get; set; } = new();
}
