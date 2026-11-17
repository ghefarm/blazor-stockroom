using System.Text.Json;

namespace Stockroom.Questions;

/// <summary>Reads the model answer leniently.</summary>
public static class AnswerParser
{
    public static IReadOnlyDictionary<string, string>? Parse(
        string text)
    {
        // Not every model server enforces the schema. Some wrap the
        // JSON in a sentence or in a Markdown code fence.
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end < start)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(
                text.AsMemory(start, end - start + 1));
            return ReadValues(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Dictionary<string, string>? ReadValues(
        JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        Dictionary<string, string> values = [];
        foreach (var property in root.EnumerateObject())
        {
            var element = property.Value;
            var value = element.ValueKind == JsonValueKind.String
                ? element.GetString()?.Trim()
                : null;
            if (!string.IsNullOrEmpty(value))
            {
                values[property.Name] = value;
            }
        }

        return values;
    }
}
