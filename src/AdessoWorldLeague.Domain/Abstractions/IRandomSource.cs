namespace AdessoWorldLeague.Domain.Abstractions;

/// <summary>
/// Abstraction over randomness so that draws can be reproduced deterministically in tests
/// while still being unpredictable in production.
/// </summary>
public interface IRandomSource
{
    /// <summary>Returns a non-negative random integer strictly smaller than <paramref name="maxExclusive"/>.</summary>
    /// <param name="maxExclusive">Exclusive upper bound; must be greater than zero.</param>
    int Next(int maxExclusive);
}
