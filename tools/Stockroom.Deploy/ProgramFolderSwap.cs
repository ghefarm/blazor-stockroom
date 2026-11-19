using System.Diagnostics;

namespace Stockroom.Deploy;

/// <summary>
/// Puts a new program folder in place with two renames. The caller
/// deletes the old folder, but only after a successful start.
/// </summary>
public sealed class ProgramFolderSwap(TimeSpan retryFor)
{
    public static ProgramFolderSwap Default { get; } =
        new(TimeSpan.FromSeconds(10));

    public void Swap(string app, string next, string old)
    {
        RetryMove(app, old);
        try
        {
            RetryMove(next, app);
        }
        catch
        {
            // Without this, there would be no program folder at all.
            RetryMove(old, app);
            throw;
        }
    }

    // Antivirus software holds fresh files for a moment.
    private void RetryMove(string from, string to)
    {
        var clock = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                Directory.Move(from, to);
                return;
            }
            catch (Exception ex) when (IsTransient(ex)
                && clock.Elapsed < retryFor)
            {
                Thread.Sleep(250);
            }
        }
    }

    private static bool IsTransient(Exception ex) =>
        ex is IOException or UnauthorizedAccessException;
}
