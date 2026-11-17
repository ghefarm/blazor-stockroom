namespace Stockroom.Core.Files;

/// <summary>
/// Answers one question: is this path inside that folder?
/// </summary>
public static class SafePath
{
    public static bool IsInside(string root, string path)
    {
        string fullRoot;
        string fullPath;
        try
        {
            fullRoot = Path.GetFullPath(root);
            fullPath = Path.GetFullPath(path, fullRoot);
        }
        catch (Exception e) when (e is ArgumentException
            or NotSupportedException or PathTooLongException)
        {
            // A path that cannot be resolved is never inside.
            return false;
        }

        // The separator stops C:\data2 from passing for C:\data.
        var prefix = Path.EndsInDirectorySeparator(fullRoot)
            ? fullRoot
            : fullRoot + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullPath.StartsWith(prefix, comparison);
    }
}
