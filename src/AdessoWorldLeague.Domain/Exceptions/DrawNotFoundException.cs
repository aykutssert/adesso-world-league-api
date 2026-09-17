namespace AdessoWorldLeague.Domain.Exceptions;

/// <summary>Raised when a draw is requested by an identifier that does not exist.</summary>
public sealed class DrawNotFoundException(Guid drawId)
    : DomainException($"Draw '{drawId}' was not found.")
{
    /// <inheritdoc />
    public override string Code => "draw.not_found";

    /// <summary>The identifier that could not be resolved.</summary>
    public Guid DrawId { get; } = drawId;
}
