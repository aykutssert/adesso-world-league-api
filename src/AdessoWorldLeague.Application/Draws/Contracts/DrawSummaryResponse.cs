namespace AdessoWorldLeague.Application.Draws.Contracts;

/// <summary>A draw without its groups, used when listing the draw history.</summary>
/// <param name="DrawId">Identifier of the draw.</param>
/// <param name="DrawnBy">The person who performed the draw.</param>
/// <param name="GroupCount">Number of groups the teams were drawn into.</param>
/// <param name="DrawnAtUtc">When the draw took place, in UTC.</param>
public sealed record DrawSummaryResponse(
    Guid DrawId,
    ParticipantResponse DrawnBy,
    int GroupCount,
    DateTimeOffset DrawnAtUtc);
