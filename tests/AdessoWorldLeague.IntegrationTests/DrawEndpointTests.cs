using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AdessoWorldLeague.IntegrationTests;

[Collection(LeagueApiCollection.Name)]
public sealed class DrawEndpointTests(LeagueApiFactory factory)
{
    private static readonly IReadOnlyDictionary<string, string> CountryByTeamName =
        LeagueSeedData.Countries
            .SelectMany(country => country.TeamNames.Select(teamName => (teamName, country.Name)))
            .ToDictionary(entry => entry.teamName, entry => entry.Name);

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public async Task Creating_a_draw_returns_groups_that_satisfy_every_league_rule(int groupCount)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount, firstName = "Ayşe", lastName = "Yılmaz" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        using var payload = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        payload.ShouldBeAValidDraw(groupCount, CountryByTeamName);
    }

    [Fact]
    public async Task A_created_draw_can_be_read_back_unchanged()
    {
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount = 8, firstName = "Mehmet", lastName = "Demir" },
            TestContext.Current.CancellationToken);

        using var createdPayload = JsonDocument.Parse(
            await created.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var drawId = createdPayload.RootElement.GetProperty("drawId").GetGuid();

        var fetched = await client.GetAsync($"/api/draws/{drawId}", TestContext.Current.CancellationToken);

        fetched.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var fetchedPayload = JsonDocument.Parse(
            await fetched.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        fetchedPayload.ShouldBeAValidDraw(8, CountryByTeamName);
        fetchedPayload.RootElement.GetProperty("drawnBy").GetProperty("firstName").GetString().ShouldBe("Mehmet");

        Flatten(fetchedPayload).ShouldBe(Flatten(createdPayload));
    }

    [Fact]
    public async Task The_person_who_performed_the_draw_is_stored_with_it()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount = 4, firstName = "  Zeynep  ", lastName = "Çelik " },
            TestContext.Current.CancellationToken);

        using var payload = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var drawId = payload.RootElement.GetProperty("drawId").GetGuid();

        var stored = await factory.QueryDatabaseAsync(dbContext => dbContext.Draws
            .AsNoTracking()
            .FirstAsync(draw => draw.Id == drawId, TestContext.Current.CancellationToken));

        stored.DrawnBy.FirstName.ShouldBe("Zeynep");
        stored.DrawnBy.LastName.ShouldBe("Çelik");
        stored.GroupCount.ShouldBe(4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(16)]
    public async Task An_unsupported_group_count_is_rejected_with_a_field_level_error(int groupCount)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount, firstName = "Ayşe", lastName = "Yılmaz" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        problem.RootElement.GetProperty("errors").GetProperty("GroupCount")
            .EnumerateArray()
            .Select(message => message.GetString())
            .ShouldContain(message => message!.Contains("4, 8", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("", "Yılmaz")]
    [InlineData("   ", "Yılmaz")]
    [InlineData("Ayşe", "")]
    public async Task A_missing_name_is_rejected(string firstName, string lastName)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount = 8, firstName, lastName },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_unknown_draw_returns_a_problem_document_with_the_domain_error_code()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/draws/{Guid.CreateVersion7()}",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        problem.RootElement.GetProperty("code").GetString().ShouldBe("draw.not_found");
    }

    [Fact]
    public async Task Turkish_team_names_are_returned_as_readable_characters()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/draws",
            new { groupCount = 8, firstName = "Ayşe", lastName = "Yılmaz" },
            TestContext.Current.CancellationToken);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        json.ShouldContain("Adesso İstanbul");
        json.ShouldNotContain("\\u0130");
    }

    [Fact]
    public async Task Draws_are_listed_newest_first_and_can_be_paged()
    {
        var client = factory.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            await client.PostAsJsonAsync(
                "/api/draws",
                new { groupCount = 8, firstName = $"Listing{i}", lastName = "Tester" },
                TestContext.Current.CancellationToken);
        }

        var response = await client.GetAsync("/api/draws?pageNumber=1&pageSize=2", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var items = payload.RootElement.GetProperty("items").EnumerateArray().ToList();

        items.Count.ShouldBe(2);
        payload.RootElement.GetProperty("totalCount").GetInt32().ShouldBeGreaterThanOrEqualTo(3);
        payload.RootElement.GetProperty("hasNextPage").GetBoolean().ShouldBeTrue();

        var timestamps = items.Select(item => item.GetProperty("drawnAtUtc").GetDateTimeOffset()).ToList();
        timestamps.ShouldBe(timestamps.OrderByDescending(timestamp => timestamp));
    }

    [Theory]
    [InlineData("pageNumber=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=1000")]
    public async Task Out_of_range_paging_is_rejected(string query)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/draws?{query}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static List<string> Flatten(JsonDocument payload) =>
    [
        .. payload.RootElement.GetProperty("groups").EnumerateArray()
            .SelectMany(group => group.GetProperty("teams").EnumerateArray()
                .Select(team => $"{group.GetProperty("groupName").GetString()}:{team.GetProperty("name").GetString()}"))
    ];
}
