using System.Text;

namespace Stockroom.Questions;

/// <summary>Builds the fixed instruction that goes before the
/// question.</summary>
public static class Instruction
{
    // No date in here. The text stays the same from one question to
    // the next, so the model server can reuse its work on it.
    private const string Rules = """
        You turn a question about a list into filter values.
        Answer only with JSON that fits the given schema.
        Use null for every field the question does not name.
        Never guess, and never add a value nobody asked for.

        """;

    public static string Create(QuestionSchema schema)
    {
        StringBuilder text = new(Rules);
        foreach (var field in schema.Fields)
        {
            text.AppendLine();
            text.AppendLine($"{field.Name}: {field.Description}");
            foreach (var choice in field.Choices)
            {
                text.AppendLine($"- {choice.Code}: {choice.Label}");
            }
        }

        return text.ToString();
    }
}
