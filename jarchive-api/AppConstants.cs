namespace Jarchive;

public static class AppConstants
{
    public const string SubjectClaimName = "sub";
    public const string NameClaimName = "name";
    public const string RoleClaimName = "role";
    public const string MetadataFilename = ".jarchive";
    public const string RootMetaFilename = ".jarchive_root";
    public const string CookieScheme = "jarchive_reader";
    public const string ReaderPolicy = "reader_policy";
    public const string OpenIdSettingsKey = "OpenId";
    public const string ShareCodeCachePrefix = "share:";
    public const string DataProtectionPurpose = "_dp:jArchive";
}

public static class Message
{
    public const string FolderNotFound = "FolderNotFound";
    public const string PermissionDenied = "PermissionDenied";
    public const string FileRenameFailed = "FileRenameFailed";
    public const string InvalidAccessToken = "InvalidAccessToken";
    public const string UpdateHadCollision = "UpdateHadCollision";
    public const string RequestNotMultipart = "RequestNotMultipart";
    public const string FilesizeExceedsLimit = "FilesizeExceedsLimit";
    public const string UploadMissingFilename = "UploadMissingFilename";
    public const string UploadFilenameInvalid = "UploadFilenameInvalid";
    public const string UploadFailed = "UploadFailed";
}
