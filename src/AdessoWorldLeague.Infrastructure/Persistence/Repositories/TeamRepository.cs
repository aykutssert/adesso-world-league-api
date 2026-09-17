using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Domain.Teams;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITeamRepository"/>.</summary>
public sealed class TeamRepository(ApplicationDbContext dbContext) : ITeamRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Team>> GetLeaguePoolAsync(CancellationToken cancellationToken) =>
        await dbContext.Teams
            .AsNoTracking()
            .Include(team => team.Country)
            .OrderBy(team => team.Name)
            .ToListAsync(cancellationToken);
}
