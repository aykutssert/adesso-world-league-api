using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Domain.Draws.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdessoWorldLeague.Application.Draws.EventHandlers;

/// <summary>
/// Reacts to a completed draw. Today it only writes an audit line; the point of routing domain events
/// through the mediator is that anything else the business asks for later - a notification, an outbox
/// message - plugs in here without touching the draw itself.
/// </summary>
public sealed class DrawCompletedDomainEventHandler(ILogger<DrawCompletedDomainEventHandler> logger)
    : INotificationHandler<DomainEventNotification<DrawCompletedDomainEvent>>
{
    /// <inheritdoc />
    public Task Handle(
        DomainEventNotification<DrawCompletedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var domainEvent = notification.DomainEvent;

        logger.LogInformation(
            "Draw {DrawId} was performed by {DrawnBy} at {OccurredAtUtc:O} into {GroupCount} groups.",
            domainEvent.DrawId,
            domainEvent.DrawnBy,
            domainEvent.OccurredAtUtc,
            domainEvent.GroupCount);

        return Task.CompletedTask;
    }
}
