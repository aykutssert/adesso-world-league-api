using AdessoWorldLeague.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AdessoWorldLeague.Api.ErrorHandling;

/// <summary>
/// Maps violated domain rules onto HTTP responses, keeping the domain's own error code in the payload so
/// clients can branch on something stable instead of parsing prose.
/// </summary>
public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (exception is not DomainException domainException)
        {
            return false;
        }

        var statusCode = domainException switch
        {
            DrawNotFoundException => StatusCodes.Status404NotFound,
            InvalidGroupCountException or InvalidParticipantNameException => StatusCodes.Status400BadRequest,

            // An infeasible draw or a broken team pool means the data or the engine is wrong, not the caller.
            DrawInfeasibleException or InvalidTeamPoolException => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest,
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Domain rule '{Code}' failed unexpectedly.", domainException.Code);
        }
        else
        {
            logger.LogInformation("Rejected request: {Code} - {Message}", domainException.Code, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == StatusCodes.Status404NotFound ? "Not found." : "The request could not be completed.",
                Detail = domainException.Message,
                Extensions = { ["code"] = domainException.Code },
            },
        });
    }
}
