using MediatR;

namespace AdessoWorldLeague.Application.Abstractions.Messaging;

/// <summary>A request that changes state and therefore runs inside a unit of work.</summary>
/// <typeparam name="TResponse">Type returned once the command has been handled.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>;

/// <summary>Handles a <see cref="ICommand{TResponse}"/>.</summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">Type returned once the command has been handled.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;
