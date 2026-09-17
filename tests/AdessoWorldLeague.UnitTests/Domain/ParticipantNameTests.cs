using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Exceptions;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

public sealed class ParticipantNameTests
{
    [Fact]
    public void Create_trims_and_collapses_whitespace()
    {
        var name = ParticipantName.Create("  Ayşe   Nur ", " Yılmaz  ");

        name.FirstName.ShouldBe("Ayşe Nur");
        name.LastName.ShouldBe("Yılmaz");
        name.FullName.ShouldBe("Ayşe Nur Yılmaz");
    }

    [Theory]
    [InlineData(null, "Yılmaz")]
    [InlineData("", "Yılmaz")]
    [InlineData("   ", "Yılmaz")]
    [InlineData("Ayşe", null)]
    [InlineData("Ayşe", "")]
    [InlineData("Ayşe", "\t")]
    public void Create_requires_both_parts(string? firstName, string? lastName) =>
        Should.Throw<InvalidParticipantNameException>(() => ParticipantName.Create(firstName, lastName));

    [Fact]
    public void Create_rejects_names_that_are_too_long()
    {
        var tooLong = new string('a', ParticipantName.MaxPartLength + 1);

        Should.Throw<InvalidParticipantNameException>(() => ParticipantName.Create(tooLong, "Yılmaz"));
        Should.Throw<InvalidParticipantNameException>(() => ParticipantName.Create("Ayşe", tooLong));
    }

    [Fact]
    public void Names_with_the_same_parts_are_equal()
    {
        ParticipantName.Create("Ayşe", "Yılmaz").ShouldBe(ParticipantName.Create(" Ayşe ", " Yılmaz "));
    }
}
