namespace AdessoWorldLeague.Domain.Exceptions;

/// <summary>
/// Raised when the draw engine reaches a state where no remaining team can legally be placed
/// into the group currently being filled.
/// </summary>
/// <remarks>
/// With the "most constrained country first" selection strategy this situation is unreachable for a
/// valid team pool; the exception exists as an explicit safety net rather than as an expected outcome.
/// </remarks>
public sealed class DrawInfeasibleException(string message) : DomainException(message)
{
    /// <inheritdoc />
    public override string Code => "draw.infeasible";
}
