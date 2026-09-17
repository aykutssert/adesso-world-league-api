using AdessoWorldLeague.Domain.Common;
using MediatR;

namespace AdessoWorldLeague.Application.Abstractions.Messaging;

/// <summary>
/// Carries a domain event through the mediator.
/// </summary>
/// <remarks>
/// The domain deliberately knows nothing about MediatR: <see cref="IDomainEvent"/> is a plain marker
/// interface. This wrapper is the adapter that lets the application layer subscribe to domain events
/// without that dependency leaking inwards.
/// </remarks>
/// <typeparam name="TDomainEvent">The wrapped domain event type.</typeparam>
/// <param name="DomainEvent">The event that was raised.</param>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
