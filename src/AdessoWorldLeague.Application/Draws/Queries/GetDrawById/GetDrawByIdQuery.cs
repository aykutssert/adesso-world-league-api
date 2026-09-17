using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Draws.Contracts;

namespace AdessoWorldLeague.Application.Draws.Queries.GetDrawById;

/// <summary>Reads a stored draw with all of its groups and teams.</summary>
/// <param name="DrawId">Identifier of the draw.</param>
public sealed record GetDrawByIdQuery(Guid DrawId) : IQuery<DrawResponse>;
