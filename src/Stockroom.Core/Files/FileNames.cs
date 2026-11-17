namespace Stockroom.Core.Files;

public static class FileNames
{
    /// <summary>
    /// Keeps only the last part of a name that came from outside and
    /// replaces every character a file name cannot have.
    /// </summary>
    public static string MakeSafe(string name, string fallback)
    {
        // A browser sends only the name. A hand-made request can
        // send a whole path, with either kind of slash.
        var last = Path.GetFileName(name.Replace('\\', '/'));
        var invalid = Path.GetInvalidFileNameChars();
        char[] chars =
            [.. last.Select(c => invalid.Contains(c) ? '_' : c)];
        var safe = new string(chars).Trim(' ', '.');
        return safe.Length == 0 ? fallback : safe;
    }
}
