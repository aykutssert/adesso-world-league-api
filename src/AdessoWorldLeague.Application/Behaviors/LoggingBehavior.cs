using MediatR;
using Microsoft.Extensions.Logging;

namespace AdessoWorldLeague.Application.Behaviors;

/// <summary>Logs the lifecycle of every request handled through the mediator.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName}.", requestName);

        try
        {
            var response = await next(cancellationToken);

            logger.LogInformation("Handled {RequestName}.", requestName);

            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "{RequestName} failed.", requestName);
            throw;
        }
    }
}
