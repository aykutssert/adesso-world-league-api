using AdessoWorldLeague.Domain.Draws;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

/// <summary>Maps the <see cref="Draw"/> aggregate root.</summary>
public sealed class DrawConfiguration : IEntityTypeConfiguration<Draw>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Draw> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("draws");

        builder.HasKey(draw => draw.Id);

        builder.Property(draw => draw.Id).HasColumnName("id");

        builder.Property(draw => draw.GroupCount).HasColumnName("group_count").IsRequired();

        builder.Property(draw => draw.DrawnAtUtc).HasColumnName("drawn_at_utc").IsRequired();

        // The participant is a value object: it has no identity of its own and lives in the draw's row.
        builder.OwnsOne(draw => draw.DrawnBy, drawnBy =>
        {
            drawnBy.Property(name => name.FirstName)
                .HasColumnName("drawn_by_first_name")
                .HasMaxLength(ParticipantName.MaxPartLength)
                .IsRequired();

            drawnBy.Property(name => name.LastName)
                .HasColumnName("drawn_by_last_name")
                .HasMaxLength(ParticipantName.MaxPartLength)
                .IsRequired();
        });

        builder.Navigation(draw => draw.DrawnBy).IsRequired();

        builder.Ignore(draw => draw.DomainEvents);

        builder.HasMany(draw => draw.Groups)
            .WithOne()
            .HasForeignKey(group => group.DrawId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Draw.Groups))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(draw => draw.DrawnAtUtc).IsDescending();
    }
}
