namespace AdessoWorldLeague.Domain.Abstractions;

/// <summary>Abstraction over the system clock, kept out of the domain for testability.</summary>
public interface IDateTimeProvider
{
    /// <summary>Current instant in UTC.</summary>
    DateTimeOffset UtcNow { get; }
}
