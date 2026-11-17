using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Stockroom.Core.Tests;

/// <summary>
/// Counts the queries EF Core sends, so a test can prove that the
/// number of queries does not grow with the number of rows.
/// </summary>
public sealed class CommandCounter : DbCommandInterceptor
{
    private readonly ConcurrentQueue<string> commands = new();

    public IReadOnlyCollection<string> Commands => commands;

    public void Clear() => commands.Clear();

    public override ValueTask<InterceptionResult<DbDataReader>>
        ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
    {
        commands.Enqueue(command.CommandText);
        return ValueTask.FromResult(result);
    }
}
