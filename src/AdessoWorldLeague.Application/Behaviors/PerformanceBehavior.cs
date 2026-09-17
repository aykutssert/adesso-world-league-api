using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdessoWorldLeague.Application.Behaviors;

/// <summary>
/// Warns when a request takes noticeably long. The draw runs a feasibility check before every pick,
/// so a regression there should be visible in the logs rather than only in a user's patience.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>Requests slower than this are logged as a warning.</summary>
    public const int SlowRequestThresholdMilliseconds = 500;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var stopwatch = Stopwatch.StartNew();

        var response = await next(cancellationToken);

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMilliseconds)
        {
            logger.LogWarning(
                "{RequestName} took {ElapsedMilliseconds} ms, which is above the {Threshold} ms threshold.",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds,
                SlowRequestThresholdMilliseconds);
        }

        return response;
    }
}
