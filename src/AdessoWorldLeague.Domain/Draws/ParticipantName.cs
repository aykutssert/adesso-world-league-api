using AdessoWorldLeague.Domain.Exceptions;

namespace AdessoWorldLeague.Domain.Draws;

/// <summary>The first and last name of the person who performed the draw.</summary>
public sealed record ParticipantName
{
    /// <summary>Maximum length accepted for each name part.</summary>
    public const int MaxPartLength = 100;

    private ParticipantName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>Given name of the person who performed the draw.</summary>
    public string FirstName { get; }

    /// <summary>Family name of the person who performed the draw.</summary>
    public string LastName { get; }

    /// <summary>Full name, used for display purposes.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Creates a participant name after trimming and collapsing whitespace.</summary>
    /// <exception cref="InvalidParticipantNameException">A name part is empty or too long.</exception>
    public static ParticipantName Create(string? firstName, string? lastName)
    {
        var first = Normalize(firstName);
        var last = Normalize(lastName);

        Validate(first, nameof(firstName));
        Validate(last, nameof(lastName));

        return new ParticipantName(first, last);
    }

    private static void Validate(string value, string partName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidParticipantNameException($"'{partName}' is required.");
        }

        if (value.Length > MaxPartLength)
        {
            throw new InvalidParticipantNameException(
                $"'{partName}' must be at most {MaxPartLength} characters long.");
        }
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(' ', parts);
    }
}
