using FluentValidation;

namespace AdessoWorldLeague.Application.Draws.Queries.GetDraws;

/// <summary>Keeps paging parameters inside sane bounds.</summary>
public sealed class GetDrawsQueryValidator : AbstractValidator<GetDrawsQuery>
{
    /// <summary>Largest page size the API serves.</summary>
    public const int MaxPageSize = 100;

    /// <summary>Creates the validator.</summary>
    public GetDrawsQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
