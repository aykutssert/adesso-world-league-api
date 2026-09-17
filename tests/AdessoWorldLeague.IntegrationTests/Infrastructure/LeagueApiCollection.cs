namespace AdessoWorldLeague.IntegrationTests.Infrastructure;

/// <summary>
/// Shares one API host and one database container across all integration tests; starting a container
/// per test class would triple the suite's runtime for no extra confidence.
/// </summary>
[CollectionDefinition(Name)]
public sealed class LeagueApiCollection : ICollectionFixture<LeagueApiFactory>
{
    /// <summary>Name of the xUnit collection.</summary>
    public const string Name = "league-api";
}
