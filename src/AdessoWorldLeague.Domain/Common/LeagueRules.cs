namespace AdessoWorldLeague.Domain.Common;

/// <summary>
/// The fixed structural rules of the Adesso World League, kept in one place so that the
/// draw engine, the validators and the seed data cannot drift apart.
/// </summary>
public static class LeagueRules
{
    /// <summary>Total number of teams competing in the league.</summary>
    public const int TotalTeams = 32;

    /// <summary>Number of countries taking part.</summary>
    public const int CountryCount = 8;

    /// <summary>Number of teams every country fields.</summary>
    public const int TeamsPerCountry = TotalTeams / CountryCount;

    /// <summary>Group labels, used in order: the first group is "A", the second "B", and so on.</summary>
    public static readonly IReadOnlyList<string> GroupLabels = ["A", "B", "C", "D", "E", "F", "G", "H"];

    /// <summary>The group counts the league supports.</summary>
    public static readonly IReadOnlyList<int> AllowedGroupCounts = [4, 8];
}
