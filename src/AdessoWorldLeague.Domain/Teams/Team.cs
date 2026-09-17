using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Countries;

namespace AdessoWorldLeague.Domain.Teams;

/// <summary>A club competing in the league. Every team belongs to exactly one country.</summary>
public sealed class Team : Entity
{
    private Team(Guid id, string name, Guid countryId) : base(id)
    {
        Name = name;
        CountryId = countryId;
    }

    // Required by EF Core.
    private Team(Guid id) : base(id) => Name = string.Empty;

    /// <summary>Display name of the team, for example "Adesso İstanbul".</summary>
    public string Name { get; private set; }

    /// <summary>Identifier of the country the team represents.</summary>
    public Guid CountryId { get; private set; }

    /// <summary>The country the team represents. Populated when explicitly loaded.</summary>
    public Country? Country { get; private set; }

    /// <summary>Creates a team.</summary>
    public static Team Create(Guid id, string name, Guid countryId) => new(id, name.Trim(), countryId);
}
