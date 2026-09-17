using AdessoWorldLeague.Domain.Draws.Engine;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

/// <summary>
/// Direct tests for the check that keeps the engine out of dead ends. The scenarios below are written
/// as small hand-made positions, because that is where the check has to be exactly right.
/// </summary>
public sealed class DrawFeasibilityTests
{
    [Fact]
    public void An_untouched_league_can_always_be_drawn()
    {
        // Eight countries with four teams each, eight groups of four, nothing placed yet.
        var remaining = Enumerable.Repeat(4, 8).ToArray();
        var freeSlots = Enumerable.Repeat(4, 8).ToArray();

        DrawFeasibility.CanComplete(remaining, freeSlots, AllAllowed(8, 8)).ShouldBeTrue();
    }

    [Fact]
    public void A_finished_draw_is_trivially_complete()
    {
        DrawFeasibility.CanComplete(new int[8], new int[8], AllAllowed(8, 8)).ShouldBeTrue();
    }

    [Fact]
    public void The_classic_dead_end_is_detected()
    {
        // One group still has a free slot, and the only country with a team left is already in it.
        var remaining = new[] { 1, 0 };
        var freeSlots = new[] { 0, 1 };
        var allowed = AllAllowed(2, 2);
        allowed[0, 1] = false;

        DrawFeasibility.CanComplete(remaining, freeSlots, allowed).ShouldBeFalse();
    }

    [Fact]
    public void A_country_needing_more_groups_than_it_may_enter_is_detected()
    {
        // Two teams left of one country, but only one group will still accept that country.
        var remaining = new[] { 2, 0 };
        var freeSlots = new[] { 1, 1 };
        var allowed = AllAllowed(2, 2);
        allowed[0, 1] = false;

        DrawFeasibility.CanComplete(remaining, freeSlots, allowed).ShouldBeFalse();
    }

    [Fact]
    public void A_position_that_only_works_with_the_right_choice_is_still_reported_as_solvable()
    {
        // Country 0 must go to group 0 (group 1 rejects it) and country 1 must take group 1.
        // A greedy look would see two candidates for group 0; the flow check sees the whole picture.
        var remaining = new[] { 1, 1 };
        var freeSlots = new[] { 1, 1 };
        var allowed = AllAllowed(2, 2);
        allowed[0, 1] = false;

        DrawFeasibility.CanComplete(remaining, freeSlots, allowed).ShouldBeTrue();
    }

    [Fact]
    public void A_mid_draw_position_of_the_real_league_is_solvable()
    {
        // Eight groups, one round played: each group holds one country, three teams left per country.
        var remaining = Enumerable.Repeat(3, 8).ToArray();
        var freeSlots = Enumerable.Repeat(3, 8).ToArray();
        var allowed = AllAllowed(8, 8);

        for (var index = 0; index < 8; index++)
        {
            allowed[index, index] = false;
        }

        DrawFeasibility.CanComplete(remaining, freeSlots, allowed).ShouldBeTrue();
    }

    [Fact]
    public void A_league_larger_than_the_fixed_network_is_refused_with_a_clear_error()
    {
        // The flow network lives on the stack with room for eight countries and eight groups.
        var remaining = Enumerable.Repeat(4, 9).ToArray();
        var freeSlots = Enumerable.Repeat(4, 9).ToArray();

        Should.Throw<ArgumentOutOfRangeException>(
            () => DrawFeasibility.CanComplete(remaining, freeSlots, AllAllowed(9, 9)));
    }

    private static bool[,] AllAllowed(int countries, int groups)
    {
        var allowed = new bool[countries, groups];

        for (var country = 0; country < countries; country++)
        {
            for (var group = 0; group < groups; group++)
            {
                allowed[country, group] = true;
            }
        }

        return allowed;
    }
}
