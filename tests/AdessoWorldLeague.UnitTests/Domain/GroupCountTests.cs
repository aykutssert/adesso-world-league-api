using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Exceptions;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

public sealed class GroupCountTests
{
    [Theory]
    [InlineData(4, 8)]
    [InlineData(8, 4)]
    public void Create_derives_the_group_size_from_the_group_count(int value, int expectedTeamsPerGroup)
    {
        var groupCount = GroupCount.Create(value);

        groupCount.Value.ShouldBe(value);
        groupCount.TeamsPerGroup.ShouldBe(expectedTeamsPerGroup);
        groupCount.DistinctCountriesPerGroup.ShouldBe(expectedTeamsPerGroup);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(16)]
    [InlineData(-4)]
    public void Create_rejects_group_counts_the_league_does_not_support(int value)
    {
        var exception = Should.Throw<InvalidGroupCountException>(() => GroupCount.Create(value));

        exception.RequestedValue.ShouldBe(value);
        exception.AllowedValues.ShouldBe(LeagueRules.AllowedGroupCounts);
        exception.Code.ShouldBe("draw.invalid_group_count");
    }

    [Fact]
    public void TryCreate_reports_failure_instead_of_throwing()
    {
        GroupCount.TryCreate(5, out _).ShouldBeFalse();
        GroupCount.TryCreate(8, out var groupCount).ShouldBeTrue();
        groupCount.Value.ShouldBe(8);
    }

    [Fact]
    public void Every_allowed_group_count_divides_the_league_evenly()
    {
        foreach (var allowed in LeagueRules.AllowedGroupCounts)
        {
            (LeagueRules.TotalTeams % allowed).ShouldBe(0);

            // A group can never need more distinct countries than the league has.
            GroupCount.Create(allowed).DistinctCountriesPerGroup.ShouldBeLessThanOrEqualTo(LeagueRules.CountryCount);
        }
    }
}
