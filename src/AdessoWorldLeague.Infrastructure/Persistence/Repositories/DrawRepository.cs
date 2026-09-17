using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Domain.Draws;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IDrawRepository"/>.</summary>
public sealed class DrawRepository(ApplicationDbContext dbContext) : IDrawRepository
{
    /// <inheritdoc />
    public void Add(Draw draw) => dbContext.Draws.Add(draw);

    /// <inheritdoc />
    public async Task<Draw?> GetByIdAsync(Guid drawId, CancellationToken cancellationToken) =>
        await dbContext.Draws
            .AsNoTracking()
            .Include(draw => draw.Groups)
                .ThenInclude(group => group.Teams)
                    .ThenInclude(groupTeam => groupTeam.Team)
                        .ThenInclude(team => team!.Country)
            .AsSplitQuery()
            .FirstOrDefaultAsync(draw => draw.Id == drawId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Draw>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken) =>
        await dbContext.Draws
            .AsNoTracking()
            .OrderByDescending(draw => draw.DrawnAtUtc)
            .ThenByDescending(draw => draw.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken) =>
        await dbContext.Draws.CountAsync(cancellationToken);
}
