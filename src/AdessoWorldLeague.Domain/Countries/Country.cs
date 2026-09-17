using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Domain.Countries;

/// <summary>A country participating in the league with a fixed number of teams.</summary>
public sealed class Country : Entity
{
    private readonly List<Team> _teams = [];

    private Country(Guid id, string name, string isoCode) : base(id)
    {
        Name = name;
        IsoCode = isoCode;
    }

    // Required by EF Core.
    private Country(Guid id) : base(id)
    {
        Name = string.Empty;
        IsoCode = string.Empty;
    }

    /// <summary>Display name of the country, for example "Türkiye".</summary>
    public string Name { get; private set; }

    /// <summary>ISO 3166-1 alpha-2 code, for example "TR".</summary>
    public string IsoCode { get; private set; }

    /// <summary>Teams representing this country.</summary>
    public IReadOnlyCollection<Team> Teams => _teams.AsReadOnly();

    /// <summary>Creates a country.</summary>
    public static Country Create(Guid id, string name, string isoCode) =>
        new(id, name.Trim(), isoCode.Trim().ToUpperInvariant());
}
