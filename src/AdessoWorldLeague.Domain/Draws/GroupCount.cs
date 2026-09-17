using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Exceptions;

namespace AdessoWorldLeague.Domain.Draws;

/// <summary>
/// The number of groups a draw is performed into. Only the values allowed by
/// <see cref="LeagueRules.AllowedGroupCounts"/> can ever be constructed.
/// </summary>
public readonly record struct GroupCount
{
    private GroupCount(int value) => Value = value;

    /// <summary>The numeric group count (4 or 8).</summary>
    public int Value { get; }

    /// <summary>How many teams end up in every group.</summary>
    public int TeamsPerGroup => LeagueRules.TotalTeams / Value;

    /// <summary>
    /// Number of distinct countries that must be present in every group. Because each country
    /// fields <see cref="LeagueRules.TeamsPerCountry"/> teams and no group may contain two teams
    /// from the same country, this is simply the group size.
    /// </summary>
    public int DistinctCountriesPerGroup => TeamsPerGroup;

    /// <summary>Creates a group count, throwing when the value is not supported.</summary>
    /// <exception cref="InvalidGroupCountException">The value is not 4 or 8.</exception>
    public static GroupCount Create(int value) =>
        TryCreate(value, out var groupCount)
            ? groupCount
            : throw new InvalidGroupCountException(value, [.. LeagueRules.AllowedGroupCounts]);

    /// <summary>Attempts to create a group count without throwing.</summary>
    public static bool TryCreate(int value, out GroupCount groupCount)
    {
        if (!LeagueRules.AllowedGroupCounts.Contains(value))
        {
            groupCount = default;
            return false;
        }

        groupCount = new GroupCount(value);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
