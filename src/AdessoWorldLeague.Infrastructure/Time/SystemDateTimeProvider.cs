using AdessoWorldLeague.Domain.Abstractions;

namespace AdessoWorldLeague.Infrastructure.Time;

/// <summary>Reads the current time from the system clock, always in UTC.</summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
