using AdessoWorldLeague.Domain.Common;

namespace AdessoWorldLeague.Domain.Draws.Events;

/// <summary>Raised once a draw has been completed and is ready to be persisted.</summary>
/// <param name="DrawId">Identifier of the completed draw.</param>
/// <param name="DrawnBy">Full name of the person who performed the draw.</param>
/// <param name="GroupCount">Number of groups the teams were drawn into.</param>
/// <param name="OccurredAtUtc">When the draw took place.</param>
public sealed record DrawCompletedDomainEvent(
    Guid DrawId,
    string DrawnBy,
    int GroupCount,
    DateTimeOffset OccurredAtUtc) : IDomainEvent;
