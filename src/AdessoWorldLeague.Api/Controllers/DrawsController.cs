using AdessoWorldLeague.Api.Contracts;
using AdessoWorldLeague.Application.Draws.Contracts;
using AdessoWorldLeague.Application.Draws.Queries.GetDrawById;
using AdessoWorldLeague.Application.Draws.Queries.GetDraws;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdessoWorldLeague.Api.Controllers;

/// <summary>Performs league draws and serves the draws that were performed earlier.</summary>
[ApiController]
[Route("api/draws")]
[Produces("application/json")]
public sealed class DrawsController(ISender sender) : ControllerBase
{
    /// <summary>Draws the thirty-two teams of the league into groups and stores the result.</summary>
    /// <remarks>
    /// Teams are drawn round-robin: one team into group A, then one into B, and so on; once every group
    /// has received a team the draw returns to group A for the next round. No group ever holds two teams
    /// from the same country.
    /// </remarks>
    /// <param name="request">Group count and the name of the person performing the draw.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The draw was performed and stored.</response>
    /// <response code="400">The group count is not 4 or 8, or a name is missing.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DrawResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DrawResponse>> CreateDraw(
        [FromBody] CreateDrawRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender.Send(request.ToCommand(), cancellationToken);

        return CreatedAtAction(nameof(GetDrawById), new { drawId = response.DrawId }, response);
    }

    /// <summary>Returns a stored draw with all of its groups and teams.</summary>
    /// <param name="drawId">Identifier of the draw.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The draw was found.</response>
    /// <response code="404">No draw exists under that identifier.</response>
    [HttpGet("{drawId:guid}")]
    [ProducesResponseType(typeof(DrawResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DrawResponse>> GetDrawById(
        Guid drawId,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetDrawByIdQuery(drawId), cancellationToken));

    /// <summary>Lists the draws that were performed, newest first.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Number of draws per page, at most 100.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">A page of draws.</response>
    /// <response code="400">The paging parameters are out of range.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DrawSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<DrawSummaryResponse>>> GetDraws(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetDrawsQuery(pageNumber, pageSize), cancellationToken));
}
