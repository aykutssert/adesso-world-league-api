namespace AdessoWorldLeague.Domain.Common;

/// <summary>Marker for something noteworthy that has happened inside the domain.</summary>
public interface IDomainEvent
{
    /// <summary>Moment the event occurred, in UTC.</summary>
    DateTimeOffset OccurredAtUtc { get; }
}
