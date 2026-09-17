using AdessoWorldLeague.Domain.Abstractions;
using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Domain.Draws.Engine;

/// <summary>Performs the league draw for a given pool of teams.</summary>
public interface IGroupDrawEngine
{
    /// <summary>Draws <paramref name="teamPool"/> into <paramref name="groupCount"/> groups.</summary>
    /// <param name="groupCount">How many groups to fill.</param>
    /// <param name="teamPool">All teams of the league; must contain every country's full squad.</param>
    /// <param name="randomSource">Source of randomness, so the draw can be reproduced in tests.</param>
    /// <returns>A plan describing which team belongs to which group.</returns>
    DrawPlan Execute(GroupCount groupCount, IReadOnlyCollection<Team> teamPool, IRandomSource randomSource);
}
