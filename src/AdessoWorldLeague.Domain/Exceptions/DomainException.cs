namespace AdessoWorldLeague.Domain.Exceptions;

/// <summary>Base type for every error that represents a violated domain rule.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Stable, machine-readable error code surfaced through the API.</summary>
    public abstract string Code { get; }
}
