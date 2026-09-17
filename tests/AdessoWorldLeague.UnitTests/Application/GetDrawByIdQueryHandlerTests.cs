using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Application.Draws.Queries.GetDrawById;
using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Draws.Engine;
using AdessoWorldLeague.Domain.Exceptions;
using AdessoWorldLeague.UnitTests.TestDoubles;
using NSubstitute;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Application;

public sealed class GetDrawByIdQueryHandlerTests
{
    private readonly IDrawRepository _drawRepository = Substitute.For<IDrawRepository>();

    [Fact]
    public async Task Handle_throws_when_the_draw_does_not_exist()
    {
        var missingId = Guid.CreateVersion7();
        _drawRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Draw?)null);

        var handler = new GetDrawByIdQueryHandler(_drawRepository);

        var exception = await Should.ThrowAsync<DrawNotFoundException>(
            () => handler.Handle(new GetDrawByIdQuery(missingId), TestContext.Current.CancellationToken));

        exception.DrawId.ShouldBe(missingId);
        exception.Code.ShouldBe("draw.not_found");
    }

    [Fact]
    public async Task Handle_returns_the_groups_of_an_existing_draw()
    {
        var plan = new RoundRobinDrawEngine().Execute(
            GroupCount.Create(8),
            LeagueSeedData.CreateTeams(),
            new SeededRandomSource(seed: 3));

        var draw = Draw.Create(ParticipantName.Create("Ayşe", "Yılmaz"), plan, DateTimeOffset.UtcNow);

        _drawRepository.GetByIdAsync(draw.Id, Arg.Any<CancellationToken>()).Returns(draw);

        var response = await new GetDrawByIdQueryHandler(_drawRepository)
            .Handle(new GetDrawByIdQuery(draw.Id), TestContext.Current.CancellationToken);

        response.DrawId.ShouldBe(draw.Id);
        response.Groups.Count.ShouldBe(8);
        response.Groups.Select(group => group.GroupName).ShouldBe(LeagueRules.GroupLabels);
    }
}
