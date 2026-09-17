using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Domain.Draws;

/// <summary>
/// A single team placed into a group by a draw. The draw order is stored as well, so the sequence
/// of picks can be replayed and audited later.
/// </summary>
public sealed class DrawGroupTeam : Entity
{
    private DrawGroupTeam(
        Guid id,
        Guid drawId,
        Guid drawGroupId,
        Guid teamId,
        int selectionOrder,
        int pickNumber) : base(id)
    {
        DrawId = drawId;
        DrawGroupId = drawGroupId;
        TeamId = teamId;
        SelectionOrder = selectionOrder;
        PickNumber = pickNumber;
    }

    // Required by EF Core.
    private DrawGroupTeam(Guid id) : base(id)
    {
    }

    /// <summary>Identifier of the owning draw, carried here so a unique index can guarantee that a team
    /// is placed into at most one group per draw.</summary>
    public Guid DrawId { get; private set; }

    /// <summary>Identifier of the owning group.</summary>
    public Guid DrawGroupId { get; private set; }

    /// <summary>Identifier of the drawn team.</summary>
    public Guid TeamId { get; private set; }

    /// <summary>Zero-based position of the team inside its group, equal to the round-robin round.</summary>
    public int SelectionOrder { get; private set; }

    /// <summary>One-based position of this pick across the whole draw.</summary>
    public int PickNumber { get; private set; }

    /// <summary>The drawn team. Populated when explicitly loaded.</summary>
    public Team? Team { get; private set; }

    internal static DrawGroupTeam Create(
        Guid drawId,
        Guid drawGroupId,
        Guid teamId,
        int selectionOrder,
        int pickNumber) =>
        new(NewId(), drawId, drawGroupId, teamId, selectionOrder, pickNumber);
}
