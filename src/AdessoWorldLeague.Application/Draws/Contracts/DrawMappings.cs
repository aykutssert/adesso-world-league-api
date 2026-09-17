using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Application.Draws.Contracts;

/// <summary>
/// Maps draw aggregates onto the API contracts. Written by hand on purpose: there are three small
/// shapes here, and a hand-written projection is easier to read - and to keep honest - than a
/// convention-based mapper.
/// </summary>
public static class DrawMappings
{
    /// <summary>Projects a fully loaded draw onto its API response.</summary>
    /// <param name="draw">A draw loaded together with its groups, teams and countries.</param>
    public static DrawResponse ToResponse(this Draw draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        return new DrawResponse(
            draw.Id,
            new ParticipantResponse(draw.DrawnBy.FirstName, draw.DrawnBy.LastName),
            draw.GroupCount,
            draw.DrawnAtUtc,
            [.. draw.Groups
                .OrderBy(group => group.Position)
                .Select(group => new DrawGroupResponse(
                    group.Label,
                    [.. group.Teams
                        .OrderBy(team => team.SelectionOrder)
                        .Select(team => new DrawTeamResponse(
                            team.Team?.Name ?? string.Empty,
                            team.Team?.Country?.Name ?? string.Empty))]))]);
    }

    /// <summary>Projects a draw onto its summary, used in listings.</summary>
    public static DrawSummaryResponse ToSummaryResponse(this Draw draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        return new DrawSummaryResponse(
            draw.Id,
            new ParticipantResponse(draw.DrawnBy.FirstName, draw.DrawnBy.LastName),
            draw.GroupCount,
            draw.DrawnAtUtc);
    }

    /// <summary>
    /// Projects a freshly created draw onto its API response using the team pool it was drawn from,
    /// which avoids reloading the aggregate from the database just to read the team names back.
    /// </summary>
    /// <param name="draw">The draw that was just created.</param>
    /// <param name="teamPool">The teams the draw was performed on, loaded together with their countries.</param>
    public static DrawResponse ToResponse(this Draw draw, IReadOnlyCollection<Team> teamPool)
    {
        ArgumentNullException.ThrowIfNull(draw);
        ArgumentNullException.ThrowIfNull(teamPool);

        var teamsById = teamPool.ToDictionary(team => team.Id);

        return new DrawResponse(
            draw.Id,
            new ParticipantResponse(draw.DrawnBy.FirstName, draw.DrawnBy.LastName),
            draw.GroupCount,
            draw.DrawnAtUtc,
            [.. draw.Groups
                .OrderBy(group => group.Position)
                .Select(group => new DrawGroupResponse(
                    group.Label,
                    [.. group.Teams
                        .OrderBy(team => team.SelectionOrder)
                        .Select(team => ToTeamResponse(teamsById[team.TeamId]))]))]);
    }

    private static DrawTeamResponse ToTeamResponse(Team team) =>
        new(team.Name, team.Country?.Name ?? string.Empty);
}
