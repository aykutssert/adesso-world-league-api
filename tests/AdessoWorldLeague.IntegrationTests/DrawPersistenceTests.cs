using System.Net.Http.Json;
using System.Text.Json;
using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AdessoWorldLeague.IntegrationTests;

[Collection(LeagueApiCollection.Name)]
public sealed class DrawPersistenceTests(LeagueApiFactory factory)
{
    [Fact]
    public async Task The_migration_seeds_the_full_league()
    {
        var counts = await factory.QueryDatabaseAsync(async dbContext => (
            Countries: await dbContext.Countries.CountAsync(TestContext.Current.CancellationToken),
            Teams: await dbContext.Teams.CountAsync(TestContext.Current.CancellationToken)));

        counts.Countries.ShouldBe(LeagueRules.CountryCount);
        counts.Teams.ShouldBe(LeagueRules.TotalTeams);
    }

    [Fact]
    public async Task Every_country_fields_exactly_four_teams()
    {
        var teamsPerCountry = await factory.QueryDatabaseAsync(dbContext => dbContext.Teams
            .GroupBy(team => team.CountryId)
            .Select(group => group.Count())
            .ToListAsync(TestContext.Current.CancellationToken));

        teamsPerCountry.Count.ShouldBe(LeagueRules.CountryCount);
        teamsPerCountry.ShouldAllBe(count => count == LeagueRules.TeamsPerCountry);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public async Task A_draw_is_stored_with_its_groups_teams_and_pick_order(int groupCount)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount, firstName = "Persist", lastName = "Tester" },
            TestContext.Current.CancellationToken);

        using var payload = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var drawId = payload.RootElement.GetProperty("drawId").GetGuid();

        var stored = await factory.QueryDatabaseAsync(dbContext => dbContext.Draws
            .AsNoTracking()
            .Include(draw => draw.Groups)
                .ThenInclude(group => group.Teams)
            .AsSplitQuery()
            .FirstAsync(draw => draw.Id == drawId, TestContext.Current.CancellationToken));

        stored.Groups.Count.ShouldBe(groupCount);
        stored.Groups.SelectMany(group => group.Teams).Count().ShouldBe(LeagueRules.TotalTeams);

        // Every team appears once across the whole draw.
        stored.Groups.SelectMany(group => group.Teams).Select(team => team.TeamId).Distinct().Count()
            .ShouldBe(LeagueRules.TotalTeams);

        // The picks were recorded in round-robin order: pick n went to group (n-1) % groupCount.
        var picks = stored.Groups
            .SelectMany(group => group.Teams.Select(team => (group.Position, team.PickNumber)))
            .OrderBy(pick => pick.PickNumber)
            .ToList();

        picks.Select(pick => pick.PickNumber).ShouldBe(Enumerable.Range(1, LeagueRules.TotalTeams));

        for (var i = 0; i < picks.Count; i++)
        {
            picks[i].Position.ShouldBe(i % groupCount);
        }
    }

    [Fact]
    public async Task No_stored_group_ever_holds_two_teams_of_the_same_country()
    {
        var client = factory.CreateClient();

        await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount = 8, firstName = "Rule", lastName = "Checker" },
            TestContext.Current.CancellationToken);

        // Asked of the database rather than of the API: this covers every draw stored by every test.
        var offendingGroups = await factory.QueryDatabaseAsync(dbContext =>
            (from groupTeam in dbContext.DrawGroupTeams
             join team in dbContext.Teams on groupTeam.TeamId equals team.Id
             group team by new { groupTeam.DrawGroupId } into grouped
             where grouped.Select(team => team.CountryId).Distinct().Count() != grouped.Count()
             select grouped.Key.DrawGroupId)
            .ToListAsync(TestContext.Current.CancellationToken));

        offendingGroups.ShouldBeEmpty();
    }

    [Fact]
    public async Task Concurrent_draws_all_succeed_and_stay_consistent()
    {
        var client = factory.CreateClient();

        // Ten draws at once: the engine must be thread safe and the unique indexes must hold.
        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(index =>
            client.PostAsJsonAsync(
                "/api/draws",
                new { groupCount = index % 2 == 0 ? 4 : 8, firstName = $"Parallel{index}", lastName = "Tester" },
                TestContext.Current.CancellationToken)));

        responses.ShouldAllBe(response => response.IsSuccessStatusCode);

        var drawIds = new List<Guid>();

        foreach (var response in responses)
        {
            using var payload = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            drawIds.Add(payload.RootElement.GetProperty("drawId").GetGuid());
        }

        drawIds.Distinct().Count().ShouldBe(10);

        var storedTeamCounts = await factory.QueryDatabaseAsync(dbContext => dbContext.DrawGroupTeams
            .Where(groupTeam => drawIds.Contains(groupTeam.DrawId))
            .GroupBy(groupTeam => groupTeam.DrawId)
            .Select(group => group.Count())
            .ToListAsync(TestContext.Current.CancellationToken));

        storedTeamCounts.Count.ShouldBe(10);
        storedTeamCounts.ShouldAllBe(count => count == LeagueRules.TotalTeams);
    }

    [Fact]
    public async Task Repeated_draws_do_not_all_produce_the_same_groups()
    {
        var client = factory.CreateClient();
        var shapes = new HashSet<string>();

        for (var i = 0; i < 8; i++)
        {
            var response = await client.PostAsJsonAsync(
                "/api/draws",
                new { groupCount = 8, firstName = "Random", lastName = "Tester" },
                TestContext.Current.CancellationToken);

            using var payload = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            shapes.Add(string.Join('|', payload.RootElement.GetProperty("groups").EnumerateArray()
                .SelectMany(group => group.GetProperty("teams").EnumerateArray()
                    .Select(team => $"{group.GetProperty("groupName").GetString()}:{team.GetProperty("name").GetString()}"))));
        }

        // A draw that keeps returning identical groups is not a draw.
        shapes.Count.ShouldBeGreaterThan(1);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    public async Task Health_probes_report_a_working_service(string path)
    {
        var response = await factory.CreateClient().GetAsync(path, TestContext.Current.CancellationToken);

        response.IsSuccessStatusCode.ShouldBeTrue();
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBe("Healthy");
    }
}
