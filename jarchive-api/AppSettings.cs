namespace Jarchive;

public class AppSettings
{
    public string PathBase { get; set; } = default!;
    public string RootFolder { get; set; } = "_jarchive";
    public string MaxFileSize { get; set; } = "128m";
    public int UploadBufferSize { get; set; } = 81920;
    public bool UploadUsingSave { get; set; } = false;
    public CorsPolicyOptions Cors { get; set; } = new();
    public CacheOptions Cache { get; set; } = new();
}
