using System.Security.Cryptography;
using AdessoWorldLeague.Domain.Abstractions;

namespace AdessoWorldLeague.Infrastructure.Randomness;

/// <summary>
/// Randomness for production draws, taken from the operating system's cryptographic generator.
/// </summary>
/// <remarks>
/// <para>
/// A draw is a fairness claim: nobody should be able to predict, or later argue they could have
/// predicted, which teams ended up together. <see cref="Random"/> is seeded from a value an observer can
/// often infer, and its output is reproducible once the seed is known, so it is the wrong tool for the
/// job however convenient it is.
/// </para>
/// <para>
/// <see cref="RandomNumberGenerator.GetInt32(int)"/> is unpredictable, thread-safe, and free of modulo
/// bias. A draw asks it for roughly seventy numbers, which costs microseconds - irrelevant next to the
/// database write that follows.
/// </para>
/// </remarks>
public sealed class CryptographicRandomSource : IRandomSource
{
    /// <inheritdoc />
    public int Next(int maxExclusive) => RandomNumberGenerator.GetInt32(maxExclusive);
}
