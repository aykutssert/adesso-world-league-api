using AdessoWorldLeague.Domain.Common;

namespace AdessoWorldLeague.Domain.Draws;

/// <summary>One group of a completed draw, for example group "A".</summary>
public sealed class DrawGroup : Entity
{
    private readonly List<DrawGroupTeam> _teams = [];

    private DrawGroup(Guid id, Guid drawId, string label, int position) : base(id)
    {
        DrawId = drawId;
        Label = label;
        Position = position;
    }

    // Required by EF Core.
    private DrawGroup(Guid id) : base(id) => Label = string.Empty;

    /// <summary>Identifier of the owning draw.</summary>
    public Guid DrawId { get; private set; }

    /// <summary>Group label, for example "A".</summary>
    public string Label { get; private set; }

    /// <summary>Zero-based position of the group in draw order.</summary>
    public int Position { get; private set; }

    /// <summary>Teams assigned to the group, in the order they were drawn.</summary>
    public IReadOnlyCollection<DrawGroupTeam> Teams => _teams.AsReadOnly();

    internal static DrawGroup Create(Guid drawId, string label, int position) =>
        new(NewId(), drawId, label, position);

    internal void AddTeam(Guid teamId, int selectionOrder, int pickNumber) =>
        _teams.Add(DrawGroupTeam.Create(DrawId, Id, teamId, selectionOrder, pickNumber));
}
