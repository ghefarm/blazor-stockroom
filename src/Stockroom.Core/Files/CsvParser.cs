using System.Text;

namespace Stockroom.Core.Files;

/// <summary>
/// Splits CSV text into rows and fields: quoted fields, doubled
/// quotes and line breaks inside quotes.
/// </summary>
public static class CsvParser
{
    public static List<string[]> Parse(string text, char separator)
    {
        // Windows files end their lines with "\r\n".
        text = text.ReplaceLineEndings("\n");
        List<string[]> rows = [];
        List<string> fields = [];
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c != '"')
                {
                    field.Append(c);
                }
                else if (i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = false;
                }

                continue;
            }

            if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == separator)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\n')
            {
                EndRow(rows, fields, field);
            }
            else
            {
                field.Append(c);
            }
        }

        // Most files end with a line break, some do not.
        if (field.Length > 0 || fields.Count > 0)
        {
            EndRow(rows, fields, field);
        }

        return rows;
    }

    private static void EndRow(
        List<string[]> rows, List<string> fields, StringBuilder field)
    {
        fields.Add(field.ToString());
        field.Clear();

        // An empty line is no row, wherever it stands.
        if (fields.Count > 1 || fields[0].Length > 0)
        {
            rows.Add([.. fields]);
        }

        fields.Clear();
    }
}
