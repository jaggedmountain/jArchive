using System.Security.Claims;

namespace Jarchive;

public static class Extentions
{
    public static string Subject(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(AppConstants.SubjectClaimName) ?? "";
    }

    public static string Name(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(AppConstants.NameClaimName) ?? "";
    }

    public static string Sanitize(this string target, char[] exclude)
    {
        string p = "";

        foreach (char c in target.ToCharArray())
            if (!exclude.Contains(c))
                p += c;

        return p.Replace(" ", "_");
    }

    public static string SanitizeFilename(this string target)
    {
        return target.Sanitize(Path.GetInvalidFileNameChars());
    }

    public static string SanitizePath(this string target)
    {
        return target.Sanitize(Path.GetInvalidPathChars());
    }

    public static string FileLinkUri(this HttpRequest request, bool isPublic = false)
    {
        return string.Format("{0}://{1}{2}/{3}/{{0}}/{{1}}/{{2}}",
            request.Scheme,
            request.Host,
            request.PathBase,
            isPublic ? "public": "get"
        );
    }
}
