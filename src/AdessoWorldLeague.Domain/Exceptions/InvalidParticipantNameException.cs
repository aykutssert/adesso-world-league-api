namespace AdessoWorldLeague.Domain.Exceptions;

/// <summary>Raised when the name of the person performing the draw is missing or malformed.</summary>
public sealed class InvalidParticipantNameException(string message) : DomainException(message)
{
    /// <inheritdoc />
    public override string Code => "draw.invalid_participant_name";
}
