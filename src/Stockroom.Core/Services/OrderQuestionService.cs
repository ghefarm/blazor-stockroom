using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;
using Stockroom.Questions;

namespace Stockroom.Core.Services;

/// <summary>Turns a question into the filter of the orders list.
/// </summary>
public sealed class OrderQuestionService(
    IDbContextFactory<StockroomDbContext> dbFactory,
    QuestionReader reader)
{
    private static readonly Choice[] StatusChoices =
    [
        new(nameof(OrderStatus.Open), "open"),
        new(nameof(OrderStatus.PartlyDelivered), "partly delivered"),
        new(nameof(OrderStatus.Delivered), "delivered"),
        new(nameof(OrderStatus.Cancelled), "cancelled"),
    ];

    public async Task<OrderQuestionResult> AskAsync(
        string question, CancellationToken ct)
    {
        var suppliers = await LoadSuppliersAsync(ct);
        var schema = CreateSchema(suppliers);
        var result = await reader.ReadAsync(question, schema, ct);
        if (result.Failure is { } failure)
        {
            return new(DescribeFailure(failure), null);
        }

        var filter = CreateFilter(result, suppliers);
        if (filter == OrderListFilter.Empty)
        {
            return new(QuestionState.NotUnderstood, null);
        }

        var state = result.IsPartial
            ? QuestionState.PartlyUnderstood
            : QuestionState.Understood;
        return new(state, filter);
    }

    private static QuestionSchema CreateSchema(
        List<SupplierChoice> suppliers) => new(
    [
        QuestionField.ForChoice(
            "supplier",
            "the supplier of the order",
            [.. suppliers.Select(s => new Choice(s.Name, s.Name))]),
        QuestionField.ForChoice(
            "status", "the status of the order", StatusChoices),
        QuestionField.ForText(
            "product", "words from a product name"),
    ]);

    private static OrderListFilter CreateFilter(
        QuestionResult result, List<SupplierChoice> suppliers)
    {
        var values = result.Values;
        int? supplierId =
            values.TryGetValue("supplier", out var name)
                ? suppliers.First(s => s.Name == name).Id
                : null;
        OrderStatus? status =
            values.TryGetValue("status", out var code)
                ? Enum.Parse<OrderStatus>(code)
                : null;
        return new OrderListFilter(
            supplierId,
            status,
            result.Period?.From,
            result.Period?.To,
            values.GetValueOrDefault("product"));
    }

    private static QuestionState DescribeFailure(
        QuestionFailure failure) => failure switch
    {
        QuestionFailure.ModelUnavailable =>
            QuestionState.ModelUnavailable,
        _ => QuestionState.AnswerUnreadable,
    };

    private async Task<List<SupplierChoice>> LoadSuppliersAsync(
        CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SupplierChoice(s.Id, s.Name))
            .ToListAsync(ct);
    }

    private sealed record SupplierChoice(int Id, string Name);
}
