namespace Stockroom.Core.Lifetimes;

// Each closed generic type has its own static counter, so the
// numbers count the instances of one lifetime only.
public abstract class InstanceMarker<T>
{
    private static int created;

    public int Number { get; } = Interlocked.Increment(ref created);
}

public sealed class SingletonMarker : InstanceMarker<SingletonMarker>;

public sealed class ScopedMarker : InstanceMarker<ScopedMarker>;

public sealed class TransientMarker : InstanceMarker<TransientMarker>;
