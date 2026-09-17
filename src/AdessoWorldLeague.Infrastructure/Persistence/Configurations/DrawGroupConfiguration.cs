using AdessoWorldLeague.Domain.Draws;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="DrawGroup"/>.</summary>
public sealed class DrawGroupConfiguration : IEntityTypeConfiguration<DrawGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DrawGroup> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("draw_groups");

        builder.HasKey(group => group.Id);

        builder.Property(group => group.Id).HasColumnName("id");
        builder.Property(group => group.DrawId).HasColumnName("draw_id").IsRequired();
        builder.Property(group => group.Position).HasColumnName("position").IsRequired();

        builder.Property(group => group.Label)
            .HasColumnName("label")
            .HasMaxLength(1)
            .IsRequired();

        builder.Ignore(group => group.DomainEvents);

        // One draw cannot contain the same group twice.
        builder.HasIndex(group => new { group.DrawId, group.Label }).IsUnique();

        builder.HasMany(group => group.Teams)
            .WithOne()
            .HasForeignKey(team => team.DrawGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(DrawGroup.Teams))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
