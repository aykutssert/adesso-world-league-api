using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdessoWorldLeague.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Publishes the domain events collected by entities once their changes have been committed.
/// </summary>
/// <remarks>
/// Dispatching after the save means handlers only ever react to facts that are actually in the database;
/// dispatching before it would let a handler act on a transaction that then rolls back.
/// </remarks>
public sealed class DispatchDomainEventsInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    private readonly List<IDomainEvent> _pendingEvents = [];

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        CollectEvents(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pendingEvents.Count > 0)
        {
            var events = _pendingEvents.ToArray();
            _pendingEvents.Clear();

            await dispatcher.DispatchAsync(events, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => _pendingEvents.Clear();

    private void CollectEvents(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            _pendingEvents.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }
    }
}
