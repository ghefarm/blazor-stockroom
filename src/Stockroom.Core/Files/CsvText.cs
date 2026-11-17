using System.Text;

namespace Stockroom.Core.Files;

/// <summary>
/// Turns the bytes of a CSV file into text, and text into bytes.
/// </summary>
public static class CsvText
{
    private static readonly byte[] Utf8Bom = [0xEF, 0xBB, 0xBF];

    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith(Utf8Bom))
        {
            // Decode after the mark, so it never becomes a
            // character in front of the first column name.
            return Encoding.UTF8.GetString(bytes[Utf8Bom.Length..]);
        }

        // Without a mark, the supplier files are Latin-1.
        return Encoding.Latin1.GetString(bytes);
    }

    public static byte[] EncodeWithBom(string text)
    {
        // GetBytes never writes the mark, so it goes in first.
        return [.. Utf8Bom, .. Encoding.UTF8.GetBytes(text)];
    }
}
