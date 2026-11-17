using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace Stockroom.Questions;

/// <summary>Builds the JSON schema of the answer.</summary>
public static class AnswerFormat
{
    public static ChatResponseFormat Create(QuestionSchema schema)
    {
        JsonObject properties = new();
        JsonArray required = new();
        foreach (var field in schema.Fields)
        {
            properties[field.Name] = field.Kind == FieldKind.Choice
                ? CreateChoiceProperty(field)
                : CreateTextProperty();
            required.Add(field.Name);
        }

        JsonObject root = new()
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false,
        };
        return ChatResponseFormat.ForJsonSchema(
            JsonSerializer.SerializeToElement(root), "filter");
    }

    private static JsonObject CreateChoiceProperty(
        QuestionField field)
    {
        JsonArray allowed = new();
        foreach (var choice in field.Choices)
        {
            allowed.Add(choice.Code);
        }

        // null is how the model says "the question does not ask".
        allowed.Add((JsonNode?)null);
        return new JsonObject
        {
            ["type"] = new JsonArray("string", "null"),
            ["enum"] = allowed,
        };
    }

    private static JsonObject CreateTextProperty() => new()
    {
        ["type"] = new JsonArray("string", "null"),
    };
}
