using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcPricingHomeMultiplierConfiguration : IEntityTypeConfiguration<CdcPricingHomeMultiplier>
{
    public void Configure(EntityTypeBuilder<CdcPricingHomeMultiplier> builder)
    {
        builder.ToTable("pricing_home_multiplier", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Homident)
            .HasColumnName("homident")
            .IsRequired();

        builder.Property(e => e.StateCode)
            .HasColumnName("state_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(e => e.EffectiveDate)
            .HasColumnName("effective_date")
            .IsRequired();

        builder.Property(e => e.HomeMultiplierValue)
            .HasColumnName("home_multiplier_value")
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(e => e.FreightMultiplier)
            .HasColumnName("freight_multiplier")
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(e => e.UpgradesMultiplier)
            .HasColumnName("upgrades_multiplier")
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(e => e.WheelsAxlesMultiplier)
            .HasColumnName("wheels_axles_multiplier")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(e => e.DuesMultiplier)
            .HasColumnName("dues_multiplier")
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => e.Homident)
            .IsUnique()
            .HasDatabaseName("uq_cdc_pricing_home_multiplier_homident");

        builder.Ignore(e => e.DomainEvents);
    }
}
