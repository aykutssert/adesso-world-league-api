using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Application.Draws.Contracts;
using AdessoWorldLeague.Domain.Abstractions;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Draws.Engine;
using Microsoft.Extensions.Logging;

namespace AdessoWorldLeague.Application.Draws.Commands.CreateDraw;

/// <summary>
/// Draws the league and stores the outcome. The draw itself is pure domain logic; this handler only
/// supplies the team pool, the clock and the randomness, and hands the result to the repository.
/// </summary>
public sealed class CreateDrawCommandHandler(
    ITeamRepository teamRepository,
    IDrawRepository drawRepository,
    IGroupDrawEngine drawEngine,
    IRandomSource randomSource,
    IDateTimeProvider dateTimeProvider,
    ILogger<CreateDrawCommandHandler> logger)
    : ICommandHandler<CreateDrawCommand, DrawResponse>
{
    /// <inheritdoc />
    public async Task<DrawResponse> Handle(CreateDrawCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var groupCount = GroupCount.Create(request.GroupCount);
        var drawnBy = ParticipantName.Create(request.FirstName, request.LastName);

        var teamPool = await teamRepository.GetLeaguePoolAsync(cancellationToken);

        var plan = drawEngine.Execute(groupCount, teamPool, randomSource);
        var draw = Draw.Create(drawnBy, plan, dateTimeProvider.UtcNow);

        drawRepository.Add(draw);

        logger.LogInformation(
            "Draw {DrawId} completed by {DrawnBy} into {GroupCount} groups.",
            draw.Id,
            drawnBy.FullName,
            groupCount.Value);

        // The unit-of-work behaviour commits the transaction once this handler returns.
        return draw.ToResponse(teamPool);
    }
}
