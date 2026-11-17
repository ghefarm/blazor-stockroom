using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;
using Stockroom.Questions;

namespace Stockroom.Core.Tests.Services;

public class OrderQuestionServiceTests
{
    private TestDatabase database = null!;
    private int northfieldId;

    [SetUp]
    public async Task CreateDatabaseAsync()
    {
        database = await TestDatabase.CreateAsync();
        await using var db = database.CreateContext();
        Supplier northfield = new() { Name = "Northfield Supply" };
        db.Suppliers.AddRange(
            northfield, new Supplier { Name = "Harbor Supply" });
        await db.SaveChangesAsync();
        northfieldId = northfield.Id;
    }

    [TearDown]
    public async Task CloseDatabaseAsync()
    {
        await database.DisposeAsync();
    }

    [Test]
    public async Task AskAsync_ClearQuestion_FillsWholeFilter()
    {
        var service = CreateService(CreateChat("""
            {"supplier": "Northfield Supply", "status": "Open",
             "product": null}
            """));

        var result = await service.AskAsync(
            "open orders from Northfield Supply in March", default);

        Assert.That(result.State,
            Is.EqualTo(QuestionState.Understood));
        Assert.That(result.Filter, Is.EqualTo(new OrderListFilter(
            northfieldId,
            OrderStatus.Open,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31))));
    }

    [Test]
    public async Task AskAsync_WeakAnswers_SayWhatWasNotUnderstood()
    {
        var partly = await CreateService(CreateChat("""
            {"supplier": "Northfield Supply", "status": "Delivered",
             "product": null}
            """)).AskAsync("orders from northfield", default);
        Assert.That(partly.State,
            Is.EqualTo(QuestionState.PartlyUnderstood));
        Assert.That(partly.Filter!.SupplierId,
            Is.EqualTo(northfieldId));
        Assert.That(partly.Filter.Status, Is.Null);

        var nothing = await CreateService(CreateChat("""
            {"supplier": null, "status": null, "product": null}
            """)).AskAsync("what is the weather like", default);
        Assert.That(nothing.State,
            Is.EqualTo(QuestionState.NotUnderstood));
        Assert.That(nothing.Filter, Is.Null);

        var chat = Substitute.For<IChatClient>();
        chat.GetResponseAsync(default!, default, default)
            .ThrowsAsyncForAnyArgs(new HttpRequestException());
        var offline = await CreateService(chat)
            .AskAsync("orders from northfield", default);
        Assert.That(offline.State,
            Is.EqualTo(QuestionState.ModelUnavailable));
        Assert.That(offline.Filter, Is.Null);
    }

    private OrderQuestionService CreateService(IChatClient chat)
    {
        FakeTimeProvider time = new(
            new DateTimeOffset(2026, 7, 15, 9, 30, 0, TimeSpan.Zero));
        QuestionReader reader = new(
            chat, time, NullLogger<QuestionReader>.Instance);
        return new OrderQuestionService(
            database.CreateFactory(), reader);
    }

    private static IChatClient CreateChat(string answer)
    {
        var chat = Substitute.For<IChatClient>();
        chat.GetResponseAsync(default!, default, default)
            .ReturnsForAnyArgs(new ChatResponse(
                new ChatMessage(ChatRole.Assistant, answer)));
        return chat;
    }
}
