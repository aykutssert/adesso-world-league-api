using AdessoWorldLeague.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace AdessoWorldLeague.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the real API in-process against a real PostgreSQL instance started in a container.
/// </summary>
/// <remarks>
/// Nothing is faked here on purpose: the migrations run, the seed data is inserted, and the unique
/// indexes that enforce the league rules are the ones the application would meet in production. An
/// in-memory provider would silently drop exactly the guarantees these tests exist to check.
/// </remarks>
public sealed class LeagueApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("adesso_world_league_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    /// <summary>Starts the database container before any test runs.</summary>
    public async ValueTask InitializeAsync() => await _database.StartAsync();

    /// <summary>Runs an action against the application's database context.</summary>
    public async Task<TResult> QueryDatabaseAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await query(dbContext);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:Database", _database.GetConnectionString());
        builder.UseSetting("Database:MigrateOnStartup", "true");

        builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
    }
}
