using AdessoWorldLeague.Domain.Countries;
using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Domain.Common;

/// <summary>
/// The fixed roster of the Adesso World League: eight countries fielding four teams each.
/// Living in the domain keeps the database seed, the tests and the rules in sync by construction.
/// </summary>
public static class LeagueSeedData
{
    /// <summary>The countries of the league and the teams they field, in the order given by the challenge.</summary>
    public static readonly IReadOnlyList<CountrySeed> Countries =
    [
        new("Türkiye", "TR", ["Adesso İstanbul", "Adesso Ankara", "Adesso İzmir", "Adesso Antalya"]),
        new("Almanya", "DE", ["Adesso Berlin", "Adesso Frankfurt", "Adesso Münih", "Adesso Dortmund"]),
        new("Fransa", "FR", ["Adesso Paris", "Adesso Marsilya", "Adesso Nice", "Adesso Lyon"]),
        new("Hollanda", "NL", ["Adesso Amsterdam", "Adesso Rotterdam", "Adesso Lahey", "Adesso Eindhoven"]),
        new("Portekiz", "PT", ["Adesso Lisbon", "Adesso Porto", "Adesso Braga", "Adesso Coimbra"]),
        new("İtalya", "IT", ["Adesso Roma", "Adesso Milano", "Adesso Venedik", "Adesso Napoli"]),
        new("İspanya", "ES", ["Adesso Sevilla", "Adesso Madrid", "Adesso Barselona", "Adesso Granada"]),
        new("Belçika", "BE", ["Adesso Brüksel", "Adesso Brugge", "Adesso Gent", "Adesso Anvers"]),
    ];

    /// <summary>Builds the country entities of the league.</summary>
    public static IReadOnlyList<Country> CreateCountries() =>
        [.. Countries.Select(country => Country.Create(country.Id, country.Name, country.IsoCode))];

    /// <summary>Builds the team entities of the league.</summary>
    public static IReadOnlyList<Team> CreateTeams() =>
        [.. Countries.SelectMany(country =>
            country.TeamNames.Select(teamName => Team.Create(TeamId(country.IsoCode, teamName), teamName, country.Id)))];

    /// <summary>Deterministic identifier of a seeded country.</summary>
    public static Guid CountryId(string isoCode) => DeterministicGuid.ForSeed($"country:{isoCode}");

    /// <summary>Deterministic identifier of a seeded team.</summary>
    public static Guid TeamId(string isoCode, string teamName) =>
        DeterministicGuid.ForSeed($"team:{isoCode}:{teamName}");

    /// <summary>A country of the league together with the teams it fields.</summary>
    /// <param name="Name">Display name of the country.</param>
    /// <param name="IsoCode">ISO 3166-1 alpha-2 code.</param>
    /// <param name="TeamNames">Names of the four teams the country fields.</param>
    public sealed record CountrySeed(string Name, string IsoCode, IReadOnlyList<string> TeamNames)
    {
        /// <summary>Deterministic identifier of the country.</summary>
        public Guid Id => LeagueSeedData.CountryId(IsoCode);
    }
}
