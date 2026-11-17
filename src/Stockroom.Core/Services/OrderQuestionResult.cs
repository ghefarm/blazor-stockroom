namespace Stockroom.Core.Services;

public enum QuestionState
{
    Understood,
    PartlyUnderstood,
    NotUnderstood,
    ModelUnavailable,
    AnswerUnreadable,
}

/// <summary>Filter is null when the page should keep its filter.
/// </summary>
public sealed record OrderQuestionResult(
    QuestionState State, OrderListFilter? Filter);
