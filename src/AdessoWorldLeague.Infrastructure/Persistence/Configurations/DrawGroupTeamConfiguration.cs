using AdessoWorldLeague.Domain.Draws;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="DrawGroupTeam"/>, the placement of one team into one group of a draw.</summary>
public sealed class DrawGroupTeamConfiguration : IEntityTypeConfiguration<DrawGroupTeam>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DrawGroupTeam> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("draw_group_teams");

        builder.HasKey(team => team.Id);

        builder.Property(team => team.Id).HasColumnName("id");
        builder.Property(team => team.DrawId).HasColumnName("draw_id").IsRequired();
        builder.Property(team => team.DrawGroupId).HasColumnName("draw_group_id").IsRequired();
        builder.Property(team => team.TeamId).HasColumnName("team_id").IsRequired();
        builder.Property(team => team.SelectionOrder).HasColumnName("selection_order").IsRequired();
        builder.Property(team => team.PickNumber).HasColumnName("pick_number").IsRequired();

        builder.Ignore(team => team.DomainEvents);

        builder.HasOne(team => team.Team)
            .WithMany()
            .HasForeignKey(team => team.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // "A team belongs to exactly one group" is a rule of the challenge, so the database enforces it
        // rather than trusting the application to be correct forever.
        builder.HasIndex(team => new { team.DrawId, team.TeamId }).IsUnique();

        // Every slot of a group is filled exactly once.
        builder.HasIndex(team => new { team.DrawGroupId, team.SelectionOrder }).IsUnique();
    }
}
