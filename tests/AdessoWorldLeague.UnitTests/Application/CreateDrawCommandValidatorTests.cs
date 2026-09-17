using AdessoWorldLeague.Application.Draws.Commands.CreateDraw;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Application;

public sealed class CreateDrawCommandValidatorTests
{
    private readonly CreateDrawCommandValidator _validator = new();

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Accepts_the_supported_group_counts(int groupCount) =>
        _validator.Validate(new CreateDrawCommand(groupCount, "Ayşe", "Yılmaz")).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(-8)]
    public void Rejects_unsupported_group_counts(int groupCount)
    {
        var result = _validator.Validate(new CreateDrawCommand(groupCount, "Ayşe", "Yılmaz"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(failure => failure.PropertyName == nameof(CreateDrawCommand.GroupCount));
    }

    [Theory]
    [InlineData("", "Yılmaz")]
    [InlineData("   ", "Yılmaz")]
    [InlineData("Ayşe", "")]
    [InlineData("Ayşe", "  ")]
    public void Requires_both_name_parts(string firstName, string lastName) =>
        _validator.Validate(new CreateDrawCommand(8, firstName, lastName)).IsValid.ShouldBeFalse();

    [Fact]
    public void Rejects_names_longer_than_the_domain_allows()
    {
        var tooLong = new string('a', 101);

        _validator.Validate(new CreateDrawCommand(8, tooLong, "Yılmaz")).IsValid.ShouldBeFalse();
        _validator.Validate(new CreateDrawCommand(8, "Ayşe", tooLong)).IsValid.ShouldBeFalse();
    }
}
