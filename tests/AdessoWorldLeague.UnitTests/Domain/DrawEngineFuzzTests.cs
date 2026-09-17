using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Draws.Engine;
using AdessoWorldLeague.UnitTests.TestDoubles;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

/// <summary>
/// The draw is random, so a single happy-path assertion proves very little: a naive engine survives most
/// seeds and dies on a few. These tests hammer the engine with thousands of different seeds and assert the
/// league rules after every single one.
/// </summary>
public sealed class DrawEngineFuzzTests
{
    private const int Iterations = 25_000;

    private readonly RoundRobinDrawEngine _engine = new();

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Engine_never_dead_ends_across_thousands_of_draws(int groupCountValue)
    {
        var groupCount = GroupCount.Create(groupCountValue);
        var teamPool = LeagueSeedData.CreateTeams();

        for (var seed = 0; seed < Iterations; seed++)
        {
            var plan = _engine.Execute(groupCount, teamPool, new SeededRandomSource(seed));

            DrawPlanGuard.IsValid(plan, groupCount, teamPool)
                .ShouldBeTrue($"seed {seed} produced an invalid draw for {groupCountValue} groups");
        }
    }

    [Fact]
    public void Every_team_reaches_every_group_position_over_many_draws()
    {
        // Sanity check on fairness: over enough draws each team should be seen in every group,
        // otherwise the selection strategy is biased.
        var groupCount = GroupCount.Create(8);
        var teamPool = LeagueSeedData.CreateTeams();
        var seenGroupsByTeam = teamPool.ToDictionary(team => team.Id, _ => new HashSet<string>());

        for (var seed = 0; seed < 500; seed++)
        {
            var plan = _engine.Execute(groupCount, teamPool, new SeededRandomSource(seed));

            foreach (var group in plan.Groups)
            {
                foreach (var slot in group.Slots)
                {
                    seenGroupsByTeam[slot.TeamId].Add(group.Label);
                }
            }
        }

        seenGroupsByTeam.Values.ShouldAllBe(groups => groups.Count == LeagueRules.GroupLabels.Count);
    }
}
