using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Stockroom.Core.Services;

namespace Stockroom.Web.Components.Orders;

public partial class QuestionBox : IDisposable
{
    private string question = "";
    private bool isRunning;
    private string? message;
    private CancellationTokenSource? cancel;

    [Inject]
    private OrderQuestionService Questions { get; set; } = default!;

    [Parameter]
    public EventCallback<OrderListFilter> OnFilter { get; set; }

    public void Dispose() => cancel?.Cancel();

    private async Task AskAsync()
    {
        if (isRunning || string.IsNullOrWhiteSpace(question))
        {
            return;
        }

        isRunning = true;
        message = null;
        cancel = new CancellationTokenSource();
        try
        {
            var result = await Questions.AskAsync(
                question, cancel.Token);
            message = DescribeState(result.State);
            if (result.Filter is not null)
            {
                await OnFilter.InvokeAsync(result.Filter);
            }
        }
        catch (OperationCanceledException)
        {
            message = "Stopped.";
        }
        finally
        {
            isRunning = false;
            cancel.Dispose();
            cancel = null;
        }
    }

    private void Stop(MouseEventArgs e)
    {
        // The second click of a double-click on "Ask" lands here,
        // because the button has turned into "Stop" in between.
        if (e.Detail > 1)
        {
            return;
        }

        cancel?.Cancel();
    }

    private static string? DescribeState(QuestionState state) =>
        state switch
        {
            QuestionState.Understood => null,
            QuestionState.PartlyUnderstood =>
                "Only partly understood. Please check the filter.",
            QuestionState.NotUnderstood =>
                "Not understood. Please set the filter by hand.",
            _ => "The AI model is not available right now.",
        };
}
