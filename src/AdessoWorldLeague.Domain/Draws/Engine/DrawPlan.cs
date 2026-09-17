namespace AdessoWorldLeague.Domain.Draws.Engine;

/// <summary>
/// The in-memory outcome of a draw: which team landed in which group and in which order.
/// It is deliberately free of persistence concerns so the engine stays a pure function.
/// </summary>
/// <param name="GroupCount">The group count the plan was produced for.</param>
/// <param name="Groups">The groups, in draw order (A, B, C, ...).</param>
public sealed record DrawPlan(GroupCount GroupCount, IReadOnlyList<DrawPlanGroup> Groups);

/// <summary>A single group inside a <see cref="DrawPlan"/>.</summary>
/// <param name="Label">Group label, for example "A".</param>
/// <param name="Position">Zero-based position of the group in draw order.</param>
/// <param name="Slots">Teams assigned to the group, in the order they were drawn.</param>
public sealed record DrawPlanGroup(string Label, int Position, IReadOnlyList<DrawPlanSlot> Slots);

/// <summary>A single team placement inside a <see cref="DrawPlanGroup"/>.</summary>
/// <param name="TeamId">Identifier of the drawn team.</param>
/// <param name="CountryId">Identifier of the country the team represents.</param>
/// <param name="SelectionOrder">
/// Zero-based position of the team inside its group, which is also the round-robin round it was drawn in.
/// </param>
/// <param name="PickNumber">One-based position of the pick across the whole draw.</param>
public sealed record DrawPlanSlot(Guid TeamId, Guid CountryId, int SelectionOrder, int PickNumber);
