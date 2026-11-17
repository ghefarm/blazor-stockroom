namespace Stockroom.Core.ReadOnly;

public sealed class ReadOnlyModeException()
    : InvalidOperationException(
        "Stockroom is read-only because no paid subscription "
        + "covers today.");
