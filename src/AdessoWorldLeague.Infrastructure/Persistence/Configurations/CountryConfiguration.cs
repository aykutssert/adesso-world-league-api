using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Countries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="Country"/> and seeds the eight countries of the league.</summary>
public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("countries");

        builder.HasKey(country => country.Id);

        builder.Property(country => country.Id).HasColumnName("id");

        builder.Property(country => country.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(country => country.IsoCode)
            .HasColumnName("iso_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.HasIndex(country => country.IsoCode).IsUnique();

        builder.Ignore(country => country.DomainEvents);

        builder.HasMany(country => country.Teams)
            .WithOne(team => team.Country)
            .HasForeignKey(team => team.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata
            .FindNavigation(nameof(Country.Teams))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(LeagueSeedData.Countries.Select(country => new
        {
            country.Id,
            country.Name,
            IsoCode = country.IsoCode,
        }));
    }
}
