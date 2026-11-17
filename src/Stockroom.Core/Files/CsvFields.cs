namespace Stockroom.Core.Files;

/// <summary>
/// Prepares single values for a CSV file that people open in a
/// spreadsheet application.
/// </summary>
public static class CsvFields
{
    private static readonly char[] FormulaStarts =
        ['=', '+', '-', '@', '\t', '\r'];

    public static string ProtectText(string value)
    {
        // A spreadsheet application runs such a text as a formula.
        // Only for text columns: -12.50 must stay a number.
        if (value.Length > 0 && FormulaStarts.Contains(value[0]))
        {
            return "'" + value;
        }

        return value;
    }

    public static string Quote(string value, char separator)
    {
        if (value.IndexOfAny([separator, '"', '\r', '\n']) < 0)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
