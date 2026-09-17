using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Domain.Countries;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Teams;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Infrastructure.Persistence;

/// <summary>EF Core context for the league database.</summary>
/// <param name="options">Context options supplied by dependency injection.</param>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    /// <summary>Database schema every table of this application lives in.</summary>
    public const string Schema = "league";

    /// <summary>Countries taking part in the league.</summary>
    public DbSet<Country> Countries => Set<Country>();

    /// <summary>Teams competing in the league.</summary>
    public DbSet<Team> Teams => Set<Team>();

    /// <summary>Completed draws.</summary>
    public DbSet<Draw> Draws => Set<Draw>();

    /// <summary>Groups of completed draws.</summary>
    public DbSet<DrawGroup> DrawGroups => Set<DrawGroup>();

    /// <summary>Team placements of completed draws.</summary>
    public DbSet<DrawGroupTeam> DrawGroupTeams => Set<DrawGroupTeam>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
