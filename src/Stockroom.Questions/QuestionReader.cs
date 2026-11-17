using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Stockroom.Questions;

/// <summary>Turns a question into filter values: rules first, then
/// the model, then a check of every value against the question.
/// </summary>
public sealed class QuestionReader(
    IChatClient chat,
    TimeProvider time,
    ILogger<QuestionReader> logger)
{
    public async Task<QuestionResult> ReadAsync(
        string question, QuestionSchema schema, CancellationToken ct)
    {
        var period = new PeriodReader(time).Read(question);
        if (!LeavesWordsForModel(question))
        {
            return QuestionResult.FromRules(period);
        }

        string answer;
        try
        {
            answer = await AskModelAsync(question, schema, ct);
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException
            || !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "The model did not answer.");
            return QuestionResult.Failed(
                QuestionFailure.ModelUnavailable);
        }

        logger.LogInformation(
            "Question {Question} got answer {Answer}",
            question, answer);
        var values = AnswerParser.Parse(answer);
        if (values is null)
        {
            return QuestionResult.Failed(
                QuestionFailure.AnswerUnreadable);
        }

        var checkedAnswer = AnswerChecker.Check(
            question, schema, values);
        return new QuestionResult(
            period, checkedAnswer.Accepted, checkedAnswer.Dropped);
    }

    private async Task<string> AskModelAsync(
        string question, QuestionSchema schema, CancellationToken ct)
    {
        List<ChatMessage> messages =
        [
            new(ChatRole.System, Instruction.Create(schema)),
            new(ChatRole.User, question),
        ];
        ChatOptions options = new()
        {
            Temperature = 0,
            MaxOutputTokens = 150,
            ResponseFormat = AnswerFormat.Create(schema),
        };
        var response = await chat.GetResponseAsync(
            messages, options, ct);
        return response.Text;
    }

    // The rules have read the period already. If no other word is
    // left, the model has nothing to add, and we save the wait.
    private static bool LeavesWordsForModel(string question) =>
        Words.Split(question).Any(word =>
            !Words.IsFiller(word)
            && !PeriodReader.IsPeriodWord(word));
}
