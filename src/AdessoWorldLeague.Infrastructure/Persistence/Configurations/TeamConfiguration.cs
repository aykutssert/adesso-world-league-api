using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="Team"/> and seeds the thirty-two teams of the league.</summary>
public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("teams");

        builder.HasKey(team => team.Id);

        builder.Property(team => team.Id).HasColumnName("id");

        builder.Property(team => team.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(team => team.CountryId).HasColumnName("country_id").IsRequired();

        builder.HasIndex(team => team.Name).IsUnique();
        builder.HasIndex(team => team.CountryId);

        builder.Ignore(team => team.DomainEvents);

        builder.HasData(LeagueSeedData.Countries.SelectMany(country =>
            country.TeamNames.Select(teamName => new
            {
                Id = LeagueSeedData.TeamId(country.IsoCode, teamName),
                Name = teamName,
                CountryId = country.Id,
            })));
    }
}
