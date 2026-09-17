using System.ComponentModel;
using AdessoWorldLeague.Application.Draws.Commands.CreateDraw;

namespace AdessoWorldLeague.Api.Contracts;

/// <summary>The body of a draw request.</summary>
/// <param name="GroupCount">
/// Number of groups to draw into. The league supports 4 (eight teams per group) or 8 (four teams per group).
/// </param>
/// <param name="FirstName">Given name of the person performing the draw.</param>
/// <param name="LastName">Family name of the person performing the draw.</param>
public sealed record CreateDrawRequest(
    [property: DefaultValue(8)] int GroupCount,
    [property: DefaultValue("Aykut")] string FirstName,
    [property: DefaultValue("Sert")] string LastName)
{
    /// <summary>Translates the transport contract into the application command.</summary>
    public CreateDrawCommand ToCommand() => new(GroupCount, FirstName, LastName);
}
