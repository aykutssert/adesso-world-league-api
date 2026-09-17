using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Exceptions;
using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Domain.Draws.Engine;

/// <summary>
/// Verifies that a <see cref="DrawPlan"/> satisfies every rule of the challenge. The engine runs this
/// on its own output, and the test suite reuses it as the single source of truth for "a valid draw".
/// </summary>
public static class DrawPlanGuard
{
    /// <summary>Throws when the plan violates any league rule.</summary>
    /// <exception cref="DrawInfeasibleException">The plan is not a valid draw.</exception>
    public static void EnsureValid(DrawPlan plan, GroupCount groupCount, IReadOnlyCollection<Team> teamPool)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(teamPool);

        Require(
            plan.Groups.Count == groupCount.Value,
            $"Expected {groupCount.Value} groups but the plan contains {plan.Groups.Count}.");

        Require(
            plan.Groups.Select(group => group.Label).SequenceEqual(LeagueRules.GroupLabels.Take(groupCount.Value)),
            "Groups must be labelled A, B, C, ... in draw order.");

        foreach (var group in plan.Groups)
        {
            Require(
                group.Slots.Count == groupCount.TeamsPerGroup,
                $"Group '{group.Label}' holds {group.Slots.Count} teams but {groupCount.TeamsPerGroup} were expected.");

            var distinctCountries = group.Slots.Select(slot => slot.CountryId).Distinct().Count();

            Require(
                distinctCountries == groupCount.DistinctCountriesPerGroup,
                $"Group '{group.Label}' must contain {groupCount.DistinctCountriesPerGroup} distinct countries but contains {distinctCountries}.");

            Require(
                group.Slots.Select(slot => slot.SelectionOrder).SequenceEqual(Enumerable.Range(0, group.Slots.Count)),
                $"Group '{group.Label}' does not record a contiguous selection order.");
        }

        var placedTeamIds = plan.Groups.SelectMany(group => group.Slots).Select(slot => slot.TeamId).ToList();

        Require(
            placedTeamIds.Count == LeagueRules.TotalTeams,
            $"The plan places {placedTeamIds.Count} teams but the league has {LeagueRules.TotalTeams}.");

        Require(
            placedTeamIds.Distinct().Count() == placedTeamIds.Count,
            "A team was placed into more than one group.");

        Require(
            placedTeamIds.ToHashSet().SetEquals(teamPool.Select(team => team.Id)),
            "The plan does not place exactly the teams of the supplied pool.");

        // The draw is round-robin: the n-th pick overall belongs to group (n-1) % groupCount.
        var picksInOrder = plan.Groups
            .SelectMany(group => group.Slots.Select(slot => (group.Position, slot.PickNumber)))
            .OrderBy(pick => pick.PickNumber)
            .ToList();

        Require(
            picksInOrder.Select(pick => pick.PickNumber).SequenceEqual(Enumerable.Range(1, LeagueRules.TotalTeams)),
            "Pick numbers must be a contiguous sequence starting at 1.");

        for (var i = 0; i < picksInOrder.Count; i++)
        {
            Require(
                picksInOrder[i].Position == i % groupCount.Value,
                $"Pick {i + 1} went to group position {picksInOrder[i].Position} but round-robin order requires {i % groupCount.Value}.");
        }
    }

    /// <summary>Returns whether the plan satisfies every league rule.</summary>
    public static bool IsValid(DrawPlan plan, GroupCount groupCount, IReadOnlyCollection<Team> teamPool)
    {
        try
        {
            EnsureValid(plan, groupCount, teamPool);
            return true;
        }
        catch (DrawInfeasibleException)
        {
            return false;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new DrawInfeasibleException(message);
        }
    }
}
