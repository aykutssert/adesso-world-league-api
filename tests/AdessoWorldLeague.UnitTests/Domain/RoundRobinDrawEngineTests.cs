using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Draws.Engine;
using AdessoWorldLeague.Domain.Exceptions;
using AdessoWorldLeague.Domain.Teams;
using AdessoWorldLeague.UnitTests.TestDoubles;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

public sealed class RoundRobinDrawEngineTests
{
    private static readonly IReadOnlyCollection<Team> TeamPool = LeagueSeedData.CreateTeams();

    private readonly RoundRobinDrawEngine _engine = new();

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Execute_produces_a_plan_that_satisfies_every_league_rule(int groupCount)
    {
        var plan = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new SeededRandomSource(seed: 42));

        DrawPlanGuard.IsValid(plan, GroupCount.Create(groupCount), TeamPool).ShouldBeTrue();
    }

    [Theory]
    [InlineData(4, 8)]
    [InlineData(8, 4)]
    public void Execute_fills_every_group_with_the_expected_number_of_teams(int groupCount, int teamsPerGroup)
    {
        var plan = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new SeededRandomSource(seed: 7));

        plan.Groups.Count.ShouldBe(groupCount);
        plan.Groups.ShouldAllBe(group => group.Slots.Count == teamsPerGroup);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Execute_never_places_two_teams_of_the_same_country_into_one_group(int groupCount)
    {
        var plan = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new SeededRandomSource(seed: 1337));

        foreach (var group in plan.Groups)
        {
            group.Slots
                .Select(slot => slot.CountryId)
                .Distinct()
                .Count()
                .ShouldBe(group.Slots.Count, $"group '{group.Label}' repeated a country");
        }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Execute_assigns_every_team_to_exactly_one_group(int groupCount)
    {
        var plan = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new SeededRandomSource(seed: 99));

        var placedTeamIds = plan.Groups.SelectMany(group => group.Slots).Select(slot => slot.TeamId).ToList();

        placedTeamIds.Count.ShouldBe(LeagueRules.TotalTeams);
        placedTeamIds.Distinct().Count().ShouldBe(LeagueRules.TotalTeams);
        placedTeamIds.ToHashSet().SetEquals(TeamPool.Select(team => team.Id)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Execute_follows_the_round_robin_pick_order_required_by_the_challenge(int groupCount)
    {
        var plan = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new SeededRandomSource(seed: 5));

        var picks = plan.Groups
            .SelectMany(group => group.Slots.Select(slot => new { group.Position, slot.PickNumber, slot.SelectionOrder }))
            .OrderBy(pick => pick.PickNumber)
            .ToList();

        // Pick 1 -> group A, pick 2 -> group B, ... and only after the last group does the next round start.
        for (var i = 0; i < picks.Count; i++)
        {
            picks[i].Position.ShouldBe(i % groupCount);
            picks[i].SelectionOrder.ShouldBe(i / groupCount);
        }
    }

    [Fact]
    public void Execute_gives_every_group_one_team_per_country_when_four_groups_are_drawn()
    {
        // With four groups of eight and eight countries, each group must hold exactly one team per country.
        var plan = _engine.Execute(GroupCount.Create(4), TeamPool, new SeededRandomSource(seed: 2024));

        foreach (var group in plan.Groups)
        {
            group.Slots.Select(slot => slot.CountryId).Distinct().Count().ShouldBe(LeagueRules.CountryCount);
        }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Execute_is_reproducible_for_the_same_seed(int groupCount)
    {
        var first = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new SeededRandomSource(seed: 11));
        var second = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new SeededRandomSource(seed: 11));

        Flatten(first).ShouldBe(Flatten(second));
    }

    [Fact]
    public void Execute_produces_different_results_for_different_seeds()
    {
        var results = Enumerable
            .Range(0, 50)
            .Select(seed => string.Join('|', Flatten(_engine.Execute(GroupCount.Create(8), TeamPool, new SeededRandomSource(seed)))))
            .ToHashSet();

        // A draw that always returns the same groups would be a broken draw.
        results.Count.ShouldBeGreaterThan(40);
    }

    [Fact]
    public void Execute_succeeds_even_when_randomness_always_picks_the_first_candidate()
    {
        // The degenerate random source removes tie-breaking entirely; the engine must still terminate legally.
        foreach (var groupCount in LeagueRules.AllowedGroupCounts)
        {
            var plan = _engine.Execute(GroupCount.Create(groupCount), TeamPool, new AlwaysFirstRandomSource());

            DrawPlanGuard.IsValid(plan, GroupCount.Create(groupCount), TeamPool).ShouldBeTrue();
        }
    }

    [Fact]
    public void Execute_rejects_a_pool_that_does_not_contain_the_whole_league()
    {
        var incompletePool = TeamPool.Take(31).ToList();

        Should.Throw<InvalidTeamPoolException>(
            () => _engine.Execute(GroupCount.Create(8), incompletePool, new SeededRandomSource(seed: 1)));
    }

    [Fact]
    public void Execute_rejects_a_pool_where_a_country_fields_the_wrong_number_of_teams()
    {
        var countries = LeagueSeedData.Countries;
        var unbalanced = TeamPool
            .Where(team => team.CountryId != countries[0].Id)
            .Append(Team.Create(Guid.CreateVersion7(), "Adesso Bursa", countries[1].Id))
            .Append(Team.Create(Guid.CreateVersion7(), "Adesso Konya", countries[1].Id))
            .Append(Team.Create(Guid.CreateVersion7(), "Adesso Adana", countries[1].Id))
            .Append(Team.Create(Guid.CreateVersion7(), "Adesso Samsun", countries[1].Id))
            .ToList();

        Should.Throw<InvalidTeamPoolException>(
            () => _engine.Execute(GroupCount.Create(8), unbalanced, new SeededRandomSource(seed: 1)));
    }

    private static List<string> Flatten(DrawPlan plan) =>
        [.. plan.Groups.SelectMany(group => group.Slots.Select(slot => $"{group.Label}:{slot.SelectionOrder}:{slot.TeamId}"))];
}
