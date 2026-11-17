using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Stockroom.Core.ReadOnly;

public static class ReadOnlyExtensions
{
    public static bool HasReadOnlyRule(this DbContext db) =>
        FindInterceptor(db) is not null;

    /// <summary>
    /// For writes that do not pass SaveChanges: ExecuteUpdate,
    /// ExecuteDelete and files. Call it before the write.
    /// </summary>
    public static void ThrowIfReadOnly(this DbContext db)
    {
        // A context without the interceptor (tests, tools) may write.
        FindInterceptor(db)?.ThrowIfReadOnly();
    }

    private static ReadOnlyInterceptor? FindInterceptor(DbContext db)
    {
        var options = db.GetService<IDbContextOptions>();
        return options.FindExtension<CoreOptionsExtension>()?
            .Interceptors?
            .OfType<ReadOnlyInterceptor>()
            .FirstOrDefault();
    }
}
