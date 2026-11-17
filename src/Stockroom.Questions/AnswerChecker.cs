namespace Stockroom.Questions;

/// <summary>Keeps a model value only if the question names it.
/// </summary>
public static class AnswerChecker
{
    public static CheckedAnswer Check(
        string question,
        QuestionSchema schema,
        IReadOnlyDictionary<string, string> answer)
    {
        var words = Words.Split(question);
        Dictionary<string, string> accepted = [];
        List<string> dropped = [];
        foreach (var field in schema.Fields)
        {
            if (!answer.TryGetValue(field.Name, out var value))
            {
                continue;
            }

            if (IsBackedByQuestion(field, value, words))
            {
                accepted[field.Name] = value;
            }
            else
            {
                dropped.Add(field.Name);
            }
        }

        return new CheckedAnswer(accepted, dropped);
    }

    private static bool IsBackedByQuestion(
        QuestionField field,
        string value,
        IReadOnlyList<string> words)
    {
        if (field.Kind == FieldKind.FreeText)
        {
            var parts = Words.Split(value);
            return parts.Count > 0 && parts.All(words.Contains);
        }

        var choice = field.Choices
            .FirstOrDefault(option => option.Code == value);
        if (choice is null
            || !Recognition.IsNamed(words, choice, field.Choices))
        {
            return false;
        }

        // "From A or B" names two choices, and the model picked one.
        // One value per field, so the field stays empty.
        return !field.Choices.Any(other => other != choice
            && !IsPartOf(other, choice)
            && Recognition.IsNamed(words, other, field.Choices));
    }

    // "Delivered" is part of "partly delivered", not a second name.
    private static bool IsPartOf(Choice part, Choice whole)
    {
        var wholeWords = Words.Split(whole.Label);
        return Words.Split(part.Label).All(wholeWords.Contains);
    }
}

public sealed record CheckedAnswer(
    IReadOnlyDictionary<string, string> Accepted,
    IReadOnlyList<string> Dropped);
