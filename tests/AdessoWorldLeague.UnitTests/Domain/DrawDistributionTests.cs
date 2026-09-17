using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Draws.Engine;
using AdessoWorldLeague.UnitTests.TestDoubles;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

/// <summary>
/// Checks that the draw is not merely legal but also even-handed.
/// </summary>
/// <remarks>
/// The fuzz tests can only catch a broken engine that produces an invalid draw. A subtler failure - a bug
/// in the feasibility check that wrongly rejects candidates, say - would keep producing perfectly valid
/// draws while quietly biasing them towards some groups. Nothing else in the suite would notice, so these
/// tests measure the distribution itself with a chi-squared test.
/// </remarks>
public sealed class DrawDistributionTests
{
    private const int Draws = 4_000;

    /// <summary>
    /// Chi-squared threshold, above the p = 0.00001 critical value for both shapes measured here
    /// (seven degrees of freedom across the groups, three across the rounds). Deliberately far out in
    /// the tail: the engine runs on fixed seeds, so the test is reproducible and only has to separate
    /// "even" from "clearly skewed".
    /// </summary>
    private const double CriticalValue = 35.0;

    private readonly RoundRobinDrawEngine _engine = new();

    [Fact]
    public void Every_team_reaches_every_group_about_equally_often()
    {
        var groupCount = GroupCount.Create(8);
        var counts = Measure(groupCount, groupCount.Value, slot => slot.groupPosition);

        AssertEvenlySpread(counts, "group");
    }

    [Fact]
    public void Every_team_is_drawn_in_every_round_about_equally_often()
    {
        // The round a team is picked in is the other axis the draw could quietly skew.
        // With eight groups there are four rounds, one per slot of a group.
        var groupCount = GroupCount.Create(8);
        var counts = Measure(groupCount, groupCount.TeamsPerGroup, slot => slot.selectionOrder);

        AssertEvenlySpread(counts, "round");
    }

    private Dictionary<Guid, int[]> Measure(
        GroupCount groupCount,
        int bucketCount,
        Func<(int groupPosition, int selectionOrder), int> bucket)
    {
        var teamPool = LeagueSeedData.CreateTeams();
        var counts = teamPool.ToDictionary(team => team.Id, _ => new int[bucketCount]);

        for (var seed = 0; seed < Draws; seed++)
        {
            var plan = _engine.Execute(groupCount, teamPool, new SeededRandomSource(seed));

            foreach (var group in plan.Groups)
            {
                foreach (var slot in group.Slots)
                {
                    counts[slot.TeamId][bucket((group.Position, slot.SelectionOrder))]++;
                }
            }
        }

        return counts;
    }

    private static void AssertEvenlySpread(Dictionary<Guid, int[]> counts, string axis)
    {
        foreach (var (teamId, observed) in counts)
        {
            observed.Sum().ShouldBe(Draws);

            var expected = Draws / (double)observed.Length;
            var chiSquared = observed.Sum(count => Math.Pow(count - expected, 2) / expected);

            chiSquared.ShouldBeLessThan(
                CriticalValue,
                $"team {teamId} is not spread evenly across every {axis}: [{string.Join(", ", observed)}]");
        }
    }
}
