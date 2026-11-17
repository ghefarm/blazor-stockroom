using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Stockroom.Questions.Tests;

public class QuestionReaderTests
{
    private static readonly QuestionSchema Schema = new(
    [
        QuestionField.ForChoice("supplier", "the supplier",
        [
            new("Northfield Supply", "Northfield Supply"),
            new("Harbor Supply", "Harbor Supply"),
            new("Monthly Crate Service", "Monthly Crate Service"),
        ]),
        QuestionField.ForChoice("status", "the order status",
        [
            new("Open", "open"),
            new("PartlyDelivered", "partly delivered"),
            new("Delivered", "delivered"),
        ]),
        QuestionField.ForText("product", "words from a product name"),
    ]);

    [Test]
    public async Task ReadAsync_AnswerInProse_KeepsNamedValues()
    {
        var chat = CreateChat("""
            Sure! {"supplier": "Northfield Supply",
                   "status": "Open", "product": null}
            """);
        var reader = CreateReader(chat);

        var result = await reader.ReadAsync(
            "open orders from northfield in march", Schema, default);

        Assert.That(result.Values["supplier"],
            Is.EqualTo("Northfield Supply"));
        Assert.That(result.Values["status"], Is.EqualTo("Open"));
        Assert.That(result.Period!.From,
            Is.EqualTo(new DateOnly(2026, 3, 1)));
        Assert.That(result.IsPartial, Is.False);

        var partly = await CreateReader(CreateChat("""
            {"status": "PartlyDelivered"}
            """)).ReadAsync(
            "partly delivered orders", Schema, default);
        Assert.That(partly.Values["status"],
            Is.EqualTo("PartlyDelivered"));
    }

    [Test]
    public async Task ReadAsync_PeriodOnly_DoesNotCallModel()
    {
        var chat = Substitute.For<IChatClient>();
        var reader = CreateReader(chat);

        var result = await reader.ReadAsync(
            "orders from last month", Schema, default);

        Assert.That(result.Period, Is.Not.Null);
        await chat.DidNotReceiveWithAnyArgs()
            .GetResponseAsync(default!, default, default);
    }

    [Test]
    public async Task ReadAsync_BadOutcomes_NeverSetUnaskedValues()
    {
        const string question = "open orders from harbor last month";

        var invented = await CreateReader(CreateChat("""
            {"supplier": "Monthly Crate Service",
             "status": "Delivered", "product": "bolts"}
            """)).ReadAsync(question, Schema, default);
        Assert.That(invented.Values, Is.Empty);
        Assert.That(invented.Dropped,
            Is.EqualTo(new[] { "supplier", "status", "product" }));

        var prose = await CreateReader(CreateChat("I cannot help."))
            .ReadAsync(question, Schema, default);
        Assert.That(prose.Failure,
            Is.EqualTo(QuestionFailure.AnswerUnreadable));

        var timeout = await CreateReader(CreateFailingChat(
            new TaskCanceledException())).ReadAsync(
            question, Schema, default);
        Assert.That(timeout.Failure,
            Is.EqualTo(QuestionFailure.ModelUnavailable));

        var both = await CreateReader(CreateChat("""
            {"supplier": "Northfield Supply"}
            """)).ReadAsync(
            "orders from northfield or harbor", Schema, default);
        Assert.That(both.Values, Is.Empty);
        Assert.That(both.Dropped, Is.EqualTo(new[] { "supplier" }));

        using CancellationTokenSource stopped = new();
        await stopped.CancelAsync();
        Assert.ThrowsAsync<OperationCanceledException>(() =>
            CreateReader(CreateFailingChat(
                new OperationCanceledException(stopped.Token)))
            .ReadAsync(question, Schema, stopped.Token));
    }

    private static QuestionReader CreateReader(IChatClient chat) =>
        new(chat, TestTime.Create(),
            NullLogger<QuestionReader>.Instance);

    private static IChatClient CreateChat(string answer)
    {
        var chat = Substitute.For<IChatClient>();
        chat.GetResponseAsync(default!, default, default)
            .ReturnsForAnyArgs(new ChatResponse(
                new ChatMessage(ChatRole.Assistant, answer)));
        return chat;
    }

    private static IChatClient CreateFailingChat(Exception error)
    {
        var chat = Substitute.For<IChatClient>();
        chat.GetResponseAsync(default!, default, default)
            .ThrowsAsyncForAnyArgs(error);
        return chat;
    }
}
