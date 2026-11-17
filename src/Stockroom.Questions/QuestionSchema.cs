namespace Stockroom.Questions;

public enum FieldKind
{
    Choice,
    FreeText,
}

public sealed record Choice(string Code, string Label);

public sealed record QuestionField(
    string Name,
    FieldKind Kind,
    string Description,
    IReadOnlyList<Choice> Choices)
{
    public static QuestionField ForChoice(
        string name,
        string description,
        IReadOnlyList<Choice> choices) =>
        new(name, FieldKind.Choice, description, choices);

    public static QuestionField ForText(
        string name, string description) =>
        new(name, FieldKind.FreeText, description, []);
}

public sealed record QuestionSchema(
    IReadOnlyList<QuestionField> Fields);
