using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Application.Draws.Commands.CreateDraw;
using AdessoWorldLeague.Domain.Abstractions;
using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Draws.Engine;
using AdessoWorldLeague.Domain.Draws.Events;
using AdessoWorldLeague.Domain.Exceptions;
using AdessoWorldLeague.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Application;

public sealed class CreateDrawCommandHandlerTests
{
    private static readonly DateTimeOffset DrawnAt = new(2026, 3, 14, 9, 30, 0, TimeSpan.Zero);

    private readonly ITeamRepository _teamRepository = Substitute.For<ITeamRepository>();
    private readonly IDrawRepository _drawRepository = Substitute.For<IDrawRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly CreateDrawCommandHandler _handler;

    public CreateDrawCommandHandlerTests()
    {
        _teamRepository.GetLeaguePoolAsync(Arg.Any<CancellationToken>()).Returns(LeagueSeedData.CreateTeams());
        _dateTimeProvider.UtcNow.Returns(DrawnAt);

        _handler = new CreateDrawCommandHandler(
            _teamRepository,
            _drawRepository,
            new RoundRobinDrawEngine(),
            new SeededRandomSource(seed: 4),
            _dateTimeProvider,
            NullLogger<CreateDrawCommandHandler>.Instance);
    }

    [Theory]
    [InlineData(4, 8)]
    [InlineData(8, 4)]
    public async Task Handle_returns_the_groups_in_the_contract_shape(int groupCount, int teamsPerGroup)
    {
        var response = await _handler.Handle(new CreateDrawCommand(groupCount, " Ayşe ", "Yılmaz"), TestContext.Current.CancellationToken);

        response.GroupCount.ShouldBe(groupCount);
        response.DrawnAtUtc.ShouldBe(DrawnAt);
        response.DrawnBy.FirstName.ShouldBe("Ayşe");
        response.Groups.Count.ShouldBe(groupCount);
        response.Groups.Select(group => group.GroupName)
            .ShouldBe(LeagueRules.GroupLabels.Take(groupCount));
        response.Groups.ShouldAllBe(group => group.Teams.Count == teamsPerGroup);
        response.Groups.SelectMany(group => group.Teams)
            .ShouldAllBe(team => team.Name.StartsWith("Adesso", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Handle_stores_the_draw_and_raises_the_domain_event()
    {
        Draw? stored = null;
        _drawRepository.When(repository => repository.Add(Arg.Any<Draw>()))
            .Do(call => stored = call.Arg<Draw>());

        var response = await _handler.Handle(new CreateDrawCommand(8, "Ayşe", "Yılmaz"), TestContext.Current.CancellationToken);

        stored.ShouldNotBeNull();
        stored.Id.ShouldBe(response.DrawId);
        stored.Groups.Count.ShouldBe(8);
        stored.DomainEvents.OfType<DrawCompletedDomainEvent>().ShouldHaveSingleItem()
            .DrawnBy.ShouldBe("Ayşe Yılmaz");
    }

    [Fact]
    public async Task Handle_does_not_save_by_itself()
    {
        // Committing is the unit-of-work behaviour's job; a handler that saves would break that contract.
        await _handler.Handle(new CreateDrawCommand(8, "Ayşe", "Yılmaz"), TestContext.Current.CancellationToken);

        await _teamRepository.Received(1).GetLeaguePoolAsync(Arg.Any<CancellationToken>());
        _drawRepository.Received(1).Add(Arg.Any<Draw>());
    }

    [Fact]
    public async Task Handle_rejects_an_unsupported_group_count_even_without_the_validator()
    {
        // Defence in depth: the pipeline validates first, but the domain refuses to build the value at all.
        await Should.ThrowAsync<InvalidGroupCountException>(
            () => _handler.Handle(new CreateDrawCommand(5, "Ayşe", "Yılmaz"), TestContext.Current.CancellationToken));

        _drawRepository.DidNotReceive().Add(Arg.Any<Draw>());
    }

    [Fact]
    public async Task Handle_rejects_a_missing_name_even_without_the_validator()
    {
        await Should.ThrowAsync<InvalidParticipantNameException>(
            () => _handler.Handle(new CreateDrawCommand(8, "  ", "Yılmaz"), TestContext.Current.CancellationToken));

        _drawRepository.DidNotReceive().Add(Arg.Any<Draw>());
    }

    [Fact]
    public async Task Handle_never_repeats_a_country_inside_a_group()
    {
        var teamsByName = LeagueSeedData.CreateTeams().ToDictionary(team => team.Name, team => team.CountryId);

        foreach (var groupCount in LeagueRules.AllowedGroupCounts)
        {
            var response = await _handler.Handle(new CreateDrawCommand(groupCount, "Ayşe", "Yılmaz"), TestContext.Current.CancellationToken);

            foreach (var group in response.Groups)
            {
                group.Teams.Select(team => teamsByName[team.Name]).Distinct().Count()
                    .ShouldBe(group.Teams.Count);
            }
        }
    }
}
