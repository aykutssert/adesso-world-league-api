namespace AdessoWorldLeague.Domain.Exceptions;

/// <summary>
/// Raised when the set of teams handed to the draw engine cannot form a valid league,
/// for example because a country does not field the expected number of teams.
/// </summary>
public sealed class InvalidTeamPoolException(string message) : DomainException(message)
{
    /// <inheritdoc />
    public override string Code => "draw.invalid_team_pool";
}
