using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Draws.Engine;
using AdessoWorldLeague.Domain.Draws.Events;

namespace AdessoWorldLeague.Domain.Draws;

/// <summary>
/// Aggregate root recording one completed draw: who performed it, when, and the resulting groups.
/// </summary>
public sealed class Draw : Entity
{
    private readonly List<DrawGroup> _groups = [];

    private Draw(Guid id, ParticipantName drawnBy, GroupCount groupCount, DateTimeOffset drawnAtUtc) : base(id)
    {
        DrawnBy = drawnBy;
        GroupCount = groupCount.Value;
        DrawnAtUtc = drawnAtUtc;
    }

    // Required by EF Core.
    private Draw(Guid id) : base(id) => DrawnBy = null!;

    /// <summary>The person who performed the draw.</summary>
    public ParticipantName DrawnBy { get; private set; }

    /// <summary>How many groups the teams were drawn into.</summary>
    public int GroupCount { get; private set; }

    /// <summary>When the draw took place, in UTC.</summary>
    public DateTimeOffset DrawnAtUtc { get; private set; }

    /// <summary>The resulting groups, ordered A, B, C, ...</summary>
    public IReadOnlyCollection<DrawGroup> Groups => _groups.AsReadOnly();

    /// <summary>Materialises a <see cref="DrawPlan"/> produced by the draw engine into a persistable aggregate.</summary>
    /// <param name="drawnBy">The person who performed the draw.</param>
    /// <param name="plan">The plan produced by <see cref="IGroupDrawEngine"/>.</param>
    /// <param name="drawnAtUtc">The instant the draw took place.</param>
    public static Draw Create(ParticipantName drawnBy, DrawPlan plan, DateTimeOffset drawnAtUtc)
    {
        ArgumentNullException.ThrowIfNull(drawnBy);
        ArgumentNullException.ThrowIfNull(plan);

        var draw = new Draw(NewId(), drawnBy, plan.GroupCount, drawnAtUtc);

        foreach (var planGroup in plan.Groups)
        {
            var group = DrawGroup.Create(draw.Id, planGroup.Label, planGroup.Position);

            foreach (var slot in planGroup.Slots)
            {
                group.AddTeam(slot.TeamId, slot.SelectionOrder, slot.PickNumber);
            }

            draw._groups.Add(group);
        }

        draw.Raise(new DrawCompletedDomainEvent(draw.Id, drawnBy.FullName, draw.GroupCount, drawnAtUtc));

        return draw;
    }
}
