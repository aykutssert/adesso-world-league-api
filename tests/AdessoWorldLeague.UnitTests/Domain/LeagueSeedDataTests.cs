using AdessoWorldLeague.Domain.Common;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

public sealed class LeagueSeedDataTests
{
    [Fact]
    public void The_roster_matches_the_structure_the_league_requires()
    {
        LeagueSeedData.Countries.Count.ShouldBe(LeagueRules.CountryCount);
        LeagueSeedData.Countries.ShouldAllBe(country => country.TeamNames.Count == LeagueRules.TeamsPerCountry);
        LeagueSeedData.CreateTeams().Count.ShouldBe(LeagueRules.TotalTeams);
    }

    [Fact]
    public void Country_codes_and_team_names_are_unique()
    {
        LeagueSeedData.Countries.Select(country => country.IsoCode).Distinct().Count()
            .ShouldBe(LeagueRules.CountryCount);

        LeagueSeedData.Countries.SelectMany(country => country.TeamNames).Distinct().Count()
            .ShouldBe(LeagueRules.TotalTeams);
    }

    [Fact]
    public void Seed_identifiers_are_stable_across_runs()
    {
        // The migration hard-codes these values, so they must not depend on machine or run.
        var first = LeagueSeedData.CreateTeams().Select(team => team.Id).ToList();
        var second = LeagueSeedData.CreateTeams().Select(team => team.Id).ToList();

        first.ShouldBe(second);
        first.Distinct().Count().ShouldBe(LeagueRules.TotalTeams);
        LeagueSeedData.CountryId("TR").ShouldBe(LeagueSeedData.CountryId("TR"));
        LeagueSeedData.CountryId("TR").ShouldNotBe(LeagueSeedData.CountryId("DE"));
    }

    [Fact]
    public void Seed_identifiers_are_version_5_uuids()
    {
        var id = LeagueSeedData.CountryId("TR");

        // Version nibble of an RFC 4122 name-based (SHA-1) identifier.
        id.ToString()[14].ShouldBe('5');
    }
}
