using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Application.Draws.Contracts;

namespace AdessoWorldLeague.Application.Draws.Queries.GetDraws;

/// <summary>Handles <see cref="GetDrawsQuery"/>.</summary>
public sealed class GetDrawsQueryHandler(IDrawRepository drawRepository)
    : IQueryHandler<GetDrawsQuery, PagedResponse<DrawSummaryResponse>>
{
    /// <inheritdoc />
    public async Task<PagedResponse<DrawSummaryResponse>> Handle(
        GetDrawsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var totalCount = await drawRepository.CountAsync(cancellationToken);
        var draws = await drawRepository.GetPageAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResponse<DrawSummaryResponse>(
            [.. draws.Select(draw => draw.ToSummaryResponse())],
            request.PageNumber,
            request.PageSize,
            totalCount);
    }
}
