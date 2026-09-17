using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Domain.Common;
using MediatR;

namespace AdessoWorldLeague.Infrastructure.Messaging;

/// <summary>
/// Publishes domain events through MediatR by wrapping each of them in a
/// <see cref="DomainEventNotification{TDomainEvent}"/>.
/// </summary>
public sealed class MediatorDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    /// <inheritdoc />
    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;

            await publisher.Publish(notification, cancellationToken);
        }
    }
}
