using AdessoWorldLeague.Domain.Abstractions;

namespace AdessoWorldLeague.UnitTests.TestDoubles;

/// <summary>
/// A degenerate <see cref="IRandomSource"/> that always returns the first candidate. It removes every
/// trace of randomness from the draw, which makes the engine's structural guarantees easy to assert.
/// </summary>
public sealed class AlwaysFirstRandomSource : IRandomSource
{
    public int Next(int maxExclusive) => 0;
}
