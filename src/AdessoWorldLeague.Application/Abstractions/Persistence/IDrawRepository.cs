using AdessoWorldLeague.Domain.Draws;

namespace AdessoWorldLeague.Application.Abstractions.Persistence;

/// <summary>Persistence for completed draws.</summary>
public interface IDrawRepository
{
    /// <summary>Stages a new draw for insertion. The unit of work commits it.</summary>
    void Add(Draw draw);

    /// <summary>Loads a draw with its groups, teams and countries, or <c>null</c> when it does not exist.</summary>
    Task<Draw?> GetByIdAsync(Guid drawId, CancellationToken cancellationToken);

    /// <summary>Loads one page of draws, newest first, without their groups.</summary>
    Task<IReadOnlyList<Draw>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Counts the stored draws.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken);
}
