using AdessoWorldLeague.Application.Abstractions.Messaging;
using AdessoWorldLeague.Application.Draws.Contracts;

namespace AdessoWorldLeague.Application.Draws.Commands.CreateDraw;

/// <summary>Performs a league draw and stores the result.</summary>
/// <param name="GroupCount">Number of groups to draw into; the league supports 4 or 8.</param>
/// <param name="FirstName">Given name of the person performing the draw.</param>
/// <param name="LastName">Family name of the person performing the draw.</param>
public sealed record CreateDrawCommand(int GroupCount, string FirstName, string LastName)
    : ICommand<DrawResponse>;
