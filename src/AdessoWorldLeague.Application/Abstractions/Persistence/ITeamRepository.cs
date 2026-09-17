using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Application.Abstractions.Persistence;

/// <summary>Read access to the teams competing in the league.</summary>
public interface ITeamRepository
{
    /// <summary>Returns every team of the league, which is the pool a draw is performed on.</summary>
    Task<IReadOnlyList<Team>> GetLeaguePoolAsync(CancellationToken cancellationToken);
}
