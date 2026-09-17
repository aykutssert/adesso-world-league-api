using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Draws.Contracts;

namespace AdessoWorldLeague.Application.Draws.Queries.GetDraws;

/// <summary>Lists stored draws, newest first.</summary>
/// <param name="PageNumber">One-based page number.</param>
/// <param name="PageSize">Number of draws per page.</param>
public sealed record GetDrawsQuery(int PageNumber = 1, int PageSize = 20)
    : IQuery<PagedResponse<DrawSummaryResponse>>;
