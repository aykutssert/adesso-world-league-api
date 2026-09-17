using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Domain.Abstractions;
using AdessoWorldLeague.Infrastructure.Messaging;
using AdessoWorldLeague.Infrastructure.Persistence;
using AdessoWorldLeague.Infrastructure.Persistence.Interceptors;
using AdessoWorldLeague.Infrastructure.Persistence.Repositories;
using AdessoWorldLeague.Infrastructure.Randomness;
using AdessoWorldLeague.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdessoWorldLeague.Infrastructure;

/// <summary>Registers persistence and the other outward-facing services.</summary>
public static class DependencyInjection
{
    /// <summary>Name of the connection string the API reads.</summary>
    public const string ConnectionStringName = "Database";

    /// <summary>Configuration key that switches automatic migrations on.</summary>
    public const string MigrateOnStartupKey = "Database:MigrateOnStartup";

    /// <summary>Adds the database, the repositories and the supporting services.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is missing. " +
                "Set ConnectionStrings__Database in the environment or in appsettings.json.");

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IRandomSource, CryptographicRandomSource>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql
                    .MigrationsHistoryTable("__ef_migrations_history", ApplicationDbContext.Schema)

                    // A database restart or a brief network hiccup should cost a retry, not a 500.
                    .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null))
                .AddInterceptors(serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>()));

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IDrawRepository, DrawRepository>();

        // Without this the probe would report a healthy API while the database is unreachable.
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>(
            name: "database",
            tags: ["ready"]);

        if (configuration.GetValue<bool>(MigrateOnStartupKey))
        {
            services.AddHostedService<DatabaseInitializer>();
        }

        return services;
    }
}
