using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using FluentValidation;

namespace AdessoWorldLeague.Application.Draws.Commands.CreateDraw;

/// <summary>Validates the draw request before any work is done.</summary>
public sealed class CreateDrawCommandValidator : AbstractValidator<CreateDrawCommand>
{
    /// <summary>Creates the validator.</summary>
    public CreateDrawCommandValidator()
    {
        RuleFor(command => command.GroupCount)
            .Must(groupCount => LeagueRules.AllowedGroupCounts.Contains(groupCount))
            .WithMessage($"Group count must be one of: {string.Join(", ", LeagueRules.AllowedGroupCounts)}.");

        RuleFor(command => command.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(ParticipantName.MaxPartLength);

        RuleFor(command => command.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(ParticipantName.MaxPartLength);
    }
}
