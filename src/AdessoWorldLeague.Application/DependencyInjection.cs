using System.Reflection;
using AdessoWorldLeague.Application.Behaviors;
using AdessoWorldLeague.Domain.Draws.Engine;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AdessoWorldLeague.Application;

/// <summary>Registers everything the application layer needs.</summary>
public static class DependencyInjection
{
    /// <summary>Adds the mediator, its pipeline, the validators and the draw engine.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Order matters: the outermost behaviour is registered first.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        services.AddSingleton<IGroupDrawEngine, RoundRobinDrawEngine>();

        return services;
    }
}
