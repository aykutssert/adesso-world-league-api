namespace AdessoWorldLeague.Domain.Exceptions;

/// <summary>Raised when the requested number of groups is not one of the values the league allows.</summary>
public sealed class InvalidGroupCountException(int requestedValue, IReadOnlyCollection<int> allowedValues)
    : DomainException($"Group count '{requestedValue}' is not supported. Allowed values: {string.Join(", ", allowedValues)}.")
{
    /// <inheritdoc />
    public override string Code => "draw.invalid_group_count";

    /// <summary>The rejected value.</summary>
    public int RequestedValue { get; } = requestedValue;

    /// <summary>The values the league accepts.</summary>
    public IReadOnlyCollection<int> AllowedValues { get; } = allowedValues;
}
