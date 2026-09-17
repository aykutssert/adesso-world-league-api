namespace AdessoWorldLeague.Application.Draws.Contracts;

/// <summary>One page of results together with the information needed to request the next one.</summary>
/// <typeparam name="T">Type of the items on the page.</typeparam>
/// <param name="Items">The items on this page.</param>
/// <param name="PageNumber">One-based page number.</param>
/// <param name="PageSize">Maximum number of items per page.</param>
/// <param name="TotalCount">Total number of items across all pages.</param>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    /// <summary>Total number of pages available.</summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether a further page exists.</summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
