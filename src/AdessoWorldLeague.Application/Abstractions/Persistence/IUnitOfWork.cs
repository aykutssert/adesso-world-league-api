namespace AdessoWorldLeague.Application.Abstractions.Persistence;

/// <summary>Commits everything a command staged, as a single transaction.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes.</summary>
    /// <returns>The number of affected rows.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
