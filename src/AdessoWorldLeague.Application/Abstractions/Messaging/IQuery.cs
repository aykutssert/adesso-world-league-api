using MediatR;

namespace AdessoWorldLeague.Application.Abstractions.Messaging;

/// <summary>A read-only request. Queries never trigger a save.</summary>
/// <typeparam name="TResponse">Type returned by the query.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>;

/// <summary>Handles a <see cref="IQuery{TResponse}"/>.</summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">Type returned by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
