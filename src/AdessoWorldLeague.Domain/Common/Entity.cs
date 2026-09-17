namespace AdessoWorldLeague.Domain.Common;

/// <summary>
/// Base class for entities identified by a globally unique, time-ordered identifier.
/// </summary>
/// <remarks>
/// Version 7 GUIDs are used instead of version 4 because they embed a timestamp prefix,
/// which keeps clustered/B-tree index inserts sequential in PostgreSQL.
/// </remarks>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id) => Id = id;

    /// <summary>Primary identifier of the entity.</summary>
    public Guid Id { get; private set; }

    /// <summary>Domain events raised by this entity and not yet dispatched.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Creates a new time-ordered identifier.</summary>
    public static Guid NewId() => Guid.CreateVersion7();

    /// <summary>Queues a domain event for dispatch after the unit of work is committed.</summary>
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Clears the queued domain events.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
