using AdessoWorldLeague.Domain.Abstractions;

namespace AdessoWorldLeague.UnitTests.TestDoubles;

/// <summary>A deterministic <see cref="IRandomSource"/>, so a draw can be replayed exactly.</summary>
public sealed class SeededRandomSource(int seed) : IRandomSource
{
    private readonly Random _random = new(seed);

    public int Next(int maxExclusive) => _random.Next(maxExclusive);
}
