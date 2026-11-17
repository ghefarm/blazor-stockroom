using System.Collections.ObjectModel;

namespace Stockroom.Questions;

public enum QuestionFailure
{
    ModelUnavailable,
    AnswerUnreadable,
}

public sealed record QuestionResult(
    Period? Period,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlyList<string> Dropped,
    QuestionFailure? Failure = null)
{
    public bool IsPartial => Dropped.Count > 0;

    public static QuestionResult FromRules(Period? period) =>
        new(period, ReadOnlyDictionary<string, string>.Empty, []);

    public static QuestionResult Failed(QuestionFailure failure) =>
        new(null, ReadOnlyDictionary<string, string>.Empty, [],
            failure);
}
