using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws;
using AdessoWorldLeague.Domain.Draws.Engine;
using AdessoWorldLeague.Domain.Exceptions;
using AdessoWorldLeague.Domain.Teams;
using AdessoWorldLeague.UnitTests.TestDoubles;
using Shouldly;

namespace AdessoWorldLeague.UnitTests.Domain;

/// <summary>
/// The guard is the yardstick the rest of the suite measures draws against, so it gets its own tests:
/// a checker that accepts everything would make every other assertion meaningless.
/// </summary>
public sealed class DrawPlanGuardTests
{
    private static readonly IReadOnlyCollection<Team> TeamPool = LeagueSeedData.CreateTeams();
    private static readonly GroupCount EightGroups = GroupCount.Create(8);

    private static DrawPlan ValidPlan() =>
        new RoundRobinDrawEngine().Execute(EightGroups, TeamPool, new SeededRandomSource(seed: 17));

    [Fact]
    public void A_plan_from_the_engine_is_accepted()
    {
        Should.NotThrow(() => DrawPlanGuard.EnsureValid(ValidPlan(), EightGroups, TeamPool));
    }

    [Fact]
    public void A_group_holding_two_teams_of_one_country_is_rejected()
    {
        var plan = ValidPlan();
        var groupA = plan.Groups[0];
        var groupB = plan.Groups[1];

        // Give group A a second team of the country its first team represents.
        var duplicate = new DrawPlanSlot(
            groupB.Slots[0].TeamId,
            groupA.Slots[0].CountryId,
            groupA.Slots[^1].SelectionOrder,
            groupA.Slots[^1].PickNumber);

        var tampered = Tamper(plan, groupIndex: 0, slotIndex: groupA.Slots.Count - 1, duplicate);

        Should.Throw<DrawInfeasibleException>(() => DrawPlanGuard.EnsureValid(tampered, EightGroups, TeamPool));
    }

    [Fact]
    public void A_team_placed_into_two_groups_is_rejected()
    {
        var plan = ValidPlan();
        var stolen = plan.Groups[0].Slots[0];
        var target = plan.Groups[1].Slots[0];

        var tampered = Tamper(
            plan,
            groupIndex: 1,
            slotIndex: 0,
            new DrawPlanSlot(stolen.TeamId, target.CountryId, target.SelectionOrder, target.PickNumber));

        Should.Throw<DrawInfeasibleException>(() => DrawPlanGuard.EnsureValid(tampered, EightGroups, TeamPool));
    }

    [Fact]
    public void A_plan_whose_picks_ignore_the_round_robin_order_is_rejected()
    {
        var plan = ValidPlan();
        var slot = plan.Groups[0].Slots[0];

        // Group A's first pick claims to have happened after group B's first pick.
        var tampered = Tamper(
            plan,
            groupIndex: 0,
            slotIndex: 0,
            new DrawPlanSlot(slot.TeamId, slot.CountryId, slot.SelectionOrder, PickNumber: 2));

        Should.Throw<DrawInfeasibleException>(() => DrawPlanGuard.EnsureValid(tampered, EightGroups, TeamPool));
    }

    [Fact]
    public void A_plan_with_the_wrong_number_of_groups_is_rejected()
    {
        var plan = ValidPlan();
        var truncated = new DrawPlan(EightGroups, [.. plan.Groups.Take(7)]);

        Should.Throw<DrawInfeasibleException>(() => DrawPlanGuard.EnsureValid(truncated, EightGroups, TeamPool));
    }

    [Fact]
    public void A_plan_built_from_a_different_pool_is_rejected()
    {
        var foreignPool = LeagueSeedData.CreateTeams()
            .Select(team => Team.Create(Guid.CreateVersion7(), team.Name, team.CountryId))
            .ToList();

        DrawPlanGuard.IsValid(ValidPlan(), EightGroups, foreignPool).ShouldBeFalse();
    }

    private static DrawPlan Tamper(DrawPlan plan, int groupIndex, int slotIndex, DrawPlanSlot replacement)
    {
        var groups = plan.Groups.ToList();
        var slots = groups[groupIndex].Slots.ToList();
        slots[slotIndex] = replacement;
        groups[groupIndex] = groups[groupIndex] with { Slots = slots };

        return plan with { Groups = groups };
    }
}
