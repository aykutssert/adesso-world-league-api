using AdessoWorldLeague.Domain.Common;

namespace AdessoWorldLeague.Application.Abstractions.Messaging;

/// <summary>Publishes domain events raised by entities during a unit of work.</summary>
public interface IDomainEventDispatcher
{
    /// <summary>Publishes the given events to their handlers.</summary>
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
