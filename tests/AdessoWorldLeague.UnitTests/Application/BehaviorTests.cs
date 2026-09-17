using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Application.Behaviors;
using AdessoWorldLeague.Application.Draws.Commands.CreateDraw;
using AdessoWorldLeague.Application.Draws.Contracts;
using AdessoWorldLeague.Application.Draws.Queries.GetDraws;
using FluentValidation;
using MediatR;
using NSubstitute;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Application;

public sealed class UnitOfWorkBehaviorTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Commands_are_committed_exactly_once()
    {
        var behavior = new UnitOfWorkBehavior<CreateDrawCommand, DrawResponse>(_unitOfWork);

        await behavior.Handle(
            new CreateDrawCommand(8, "Ayşe", "Yılmaz"),
            _ => Task.FromResult<DrawResponse>(null!),
            TestContext.Current.CancellationToken);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Queries_never_write()
    {
        var behavior = new UnitOfWorkBehavior<GetDrawsQuery, PagedResponse<DrawSummaryResponse>>(_unitOfWork);

        await behavior.Handle(
            new GetDrawsQuery(),
            _ => Task.FromResult<PagedResponse<DrawSummaryResponse>>(null!),
            TestContext.Current.CancellationToken);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_failing_command_is_not_committed()
    {
        var behavior = new UnitOfWorkBehavior<CreateDrawCommand, DrawResponse>(_unitOfWork);

        await Should.ThrowAsync<InvalidOperationException>(() => behavior.Handle(
            new CreateDrawCommand(8, "Ayşe", "Yılmaz"),
            _ => throw new InvalidOperationException("handler failed"),
            TestContext.Current.CancellationToken));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Invalid_requests_never_reach_the_handler()
    {
        var handlerWasCalled = false;

        var behavior = new ValidationBehavior<CreateDrawCommand, DrawResponse>([new CreateDrawCommandValidator()]);

        await Should.ThrowAsync<ValidationException>(() => behavior.Handle(
            new CreateDrawCommand(5, "", ""),
            _ =>
            {
                handlerWasCalled = true;
                return Task.FromResult<DrawResponse>(null!);
            },
            TestContext.Current.CancellationToken));

        handlerWasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Every_broken_rule_is_reported_at_once()
    {
        var behavior = new ValidationBehavior<CreateDrawCommand, DrawResponse>([new CreateDrawCommandValidator()]);

        var exception = await Should.ThrowAsync<ValidationException>(() => behavior.Handle(
            new CreateDrawCommand(5, "", ""),
            _ => Task.FromResult<DrawResponse>(null!),
            TestContext.Current.CancellationToken));

        exception.Errors.Select(failure => failure.PropertyName).Distinct().ShouldBe(
            [
                nameof(CreateDrawCommand.GroupCount),
                nameof(CreateDrawCommand.FirstName),
                nameof(CreateDrawCommand.LastName),
            ],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Requests_without_a_validator_pass_through()
    {
        var behavior = new ValidationBehavior<GetDrawsQuery, PagedResponse<DrawSummaryResponse>>([]);
        var expected = new PagedResponse<DrawSummaryResponse>([], 1, 20, 0);

        var response = await behavior.Handle(
            new GetDrawsQuery(),
            _ => Task.FromResult(expected),
            TestContext.Current.CancellationToken);

        response.ShouldBe(expected);
    }
}

public sealed class PagedResponseTests
{
    [Theory]
    [InlineData(0, 20, 1, 0, false)]
    [InlineData(45, 20, 1, 3, true)]
    [InlineData(45, 20, 3, 3, false)]
    [InlineData(40, 20, 1, 2, true)]
    [InlineData(40, 20, 2, 2, false)]
    public void Page_maths_is_consistent(
        int totalCount,
        int pageSize,
        int pageNumber,
        int expectedPages,
        bool expectedHasNext)
    {
        var response = new PagedResponse<DrawSummaryResponse>([], pageNumber, pageSize, totalCount);

        response.TotalPages.ShouldBe(expectedPages);
        response.HasNextPage.ShouldBe(expectedHasNext);
    }
}
