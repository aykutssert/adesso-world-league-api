using System.Text.Json;
using Shouldly;

namespace AdessoWorldLeague.IntegrationTests.Infrastructure;

/// <summary>Assertions over the raw JSON of a draw response.</summary>
/// <remarks>
/// The tests deliberately read the payload as JSON rather than deserialising it into the application's
/// own records: the point is to verify the wire contract the challenge specifies, which a shared DTO
/// would happily keep satisfying even if it drifted.
/// </remarks>
public static class DrawResponseAssertions
{
    /// <summary>Asserts the payload has the structure the challenge asks for and obeys the league rules.</summary>
    /// <param name="payload">The raw response JSON.</param>
    /// <param name="expectedGroupCount">Number of groups the draw was requested for.</param>
    /// <param name="countryByTeamName">Country of every team in the league, used to detect repeats.</param>
    public static void ShouldBeAValidDraw(
        this JsonDocument payload,
        int expectedGroupCount,
        IReadOnlyDictionary<string, string> countryByTeamName)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(countryByTeamName);

        var groups = payload.RootElement.GetProperty("groups").EnumerateArray().ToList();
        var expectedTeamsPerGroup = 32 / expectedGroupCount;
        var expectedLabels = new[] { "A", "B", "C", "D", "E", "F", "G", "H" }.Take(expectedGroupCount);

        groups.Count.ShouldBe(expectedGroupCount);
        groups.Select(group => group.GetProperty("groupName").GetString()).ShouldBe(expectedLabels);

        var allTeamNames = new List<string>();

        foreach (var group in groups)
        {
            var teamNames = group
                .GetProperty("teams")
                .EnumerateArray()
                .Select(team => team.GetProperty("name").GetString()!)
                .ToList();

            teamNames.Count.ShouldBe(expectedTeamsPerGroup);

            var countries = teamNames.Select(name => countryByTeamName[name]).ToList();

            countries.Distinct().Count().ShouldBe(
                countries.Count,
                $"group '{group.GetProperty("groupName").GetString()}' contains two teams from one country");

            allTeamNames.AddRange(teamNames);
        }

        allTeamNames.Count.ShouldBe(32);
        allTeamNames.Distinct().Count().ShouldBe(32, "a team was placed into more than one group");
    }
}
