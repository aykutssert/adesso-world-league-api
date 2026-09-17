using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Abstractions.Persistence;
using AdessoWorldLeague.Application.Draws.Contracts;
using AdessoWorldLeague.Domain.Exceptions;

namespace AdessoWorldLeague.Application.Draws.Queries.GetDrawById;

/// <summary>Handles <see cref="GetDrawByIdQuery"/>.</summary>
public sealed class GetDrawByIdQueryHandler(IDrawRepository drawRepository)
    : IQueryHandler<GetDrawByIdQuery, DrawResponse>
{
    /// <inheritdoc />
    public async Task<DrawResponse> Handle(GetDrawByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var draw = await drawRepository.GetByIdAsync(request.DrawId, cancellationToken)
            ?? throw new DrawNotFoundException(request.DrawId);

        return draw.ToResponse();
    }
}
