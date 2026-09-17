using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdessoWorldLeague.Infrastructure.Persistence;

/// <summary>
/// Applies pending migrations at start-up when the host is configured to do so.
/// </summary>
/// <remarks>
/// This is what makes <c>docker compose up</c> a one-step experience: the API waits for PostgreSQL, creates
/// the schema and seeds the league. It is opt-in through <c>Database:MigrateOnStartup</c> because in a real
/// deployment migrations belong in a controlled release step, not in every starting replica.
/// </remarks>
public sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Applying database migrations.");

        await dbContext.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Database is up to date.");
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
