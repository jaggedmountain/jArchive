namespace Jarchive;

public class AppSettings
{
    public string PathBase { get; set; } = default!;
    public string RootFolder { get; set; } = "_jarchive";
    public string MaxFileSize { get; set; } = "128m";
    public CorsPolicyOptions Cors { get; set; } = new();
    public CacheOptions Cache { get; set; } = new();
}
