using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Abstractions.Persistence;
using MediatR;

namespace AdessoWorldLeague.Application.Behaviors;

/// <summary>
/// Commits the changes a command staged, once and in one transaction. Handlers therefore never call
/// <see cref="IUnitOfWork.SaveChangesAsync"/> themselves, and queries never write at all.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Closed generic types get their own static field, so this reflection runs once per request type.
    private static readonly bool RequestIsCommand = Array.Exists(
        typeof(TRequest).GetInterfaces(),
        contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(ICommand<>));

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var response = await next(cancellationToken);

        if (RequestIsCommand)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
