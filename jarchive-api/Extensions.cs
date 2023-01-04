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

    public static long ToByteMultiplier(this string suffix) => suffix.ToLower() switch
    {
        "k" => 0x400,
        "m" => 0x100000,
        "g" => 0x40000000,
        "t" => 0x10000000000,
        _ => 1
    };

    public static long ToSizeBi(this string token)
    {
        if (Int64.TryParse(token, out long result))
            return result;

        System.Text.RegularExpressions.Regex regex = new(
            @"^(\d+)([bkmgt])",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        var match = regex.Match(token);
        if (match.Success)
            return
                Int64.Parse(match.Groups[1].Value) *
                match.Groups[2].Value.ToByteMultiplier();

        throw new Exception($"Invalid Numeric String: [{token}]");
    }
}
