namespace AdessoWorldLeague.Application.Draws.Contracts;

/// <summary>
/// The result of a draw. The <c>groups</c> section matches the response shape given in the challenge
/// exactly; the surrounding fields identify the draw and the person who performed it.
/// </summary>
/// <param name="DrawId">Identifier under which the draw was stored.</param>
/// <param name="DrawnBy">The person who performed the draw.</param>
/// <param name="GroupCount">Number of groups the teams were drawn into.</param>
/// <param name="DrawnAtUtc">When the draw took place, in UTC.</param>
/// <param name="Groups">The groups and their teams, in draw order.</param>
public sealed record DrawResponse(
    Guid DrawId,
    ParticipantResponse DrawnBy,
    int GroupCount,
    DateTimeOffset DrawnAtUtc,
    IReadOnlyList<DrawGroupResponse> Groups);

/// <summary>A single group of a draw.</summary>
/// <param name="GroupName">Group label, for example "A".</param>
/// <param name="Teams">Teams in the group, in the order they were drawn.</param>
public sealed record DrawGroupResponse(string GroupName, IReadOnlyList<DrawTeamResponse> Teams);

/// <summary>A team inside a group.</summary>
/// <param name="Name">Display name of the team, for example "Adesso İstanbul".</param>
/// <param name="Country">Country the team represents.</param>
public sealed record DrawTeamResponse(string Name, string Country);

/// <summary>The person who performed a draw.</summary>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
public sealed record ParticipantResponse(string FirstName, string LastName);
