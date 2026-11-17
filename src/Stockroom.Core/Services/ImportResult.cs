namespace Stockroom.Core.Services;

/// <summary>
/// What an import did, or why it did nothing.
/// </summary>
public sealed record ImportResult(
    BatchResult? Prices, IReadOnlyList<string> Errors)
{
    public bool Succeeded => Errors.Count == 0;

    public static ImportResult Fail(params string[] errors) =>
        new(null, errors);
}

/// <summary>
/// The folder that keeps a copy of every imported file.
/// </summary>
public sealed record ImportArchive(string Folder);
