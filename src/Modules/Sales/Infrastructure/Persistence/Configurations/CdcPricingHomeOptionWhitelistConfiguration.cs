using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcPricingHomeOptionWhitelistConfiguration : IEntityTypeConfiguration<CdcPricingHomeOptionWhitelist>
{
    public void Configure(EntityTypeBuilder<CdcPricingHomeOptionWhitelist> builder)
    {
        builder.ToTable("pricing_home_option_whitelist", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Howident)
            .HasColumnName("howident")
            .IsRequired();

        builder.Property(e => e.PlantNumber)
            .HasColumnName("plant_number")
            .IsRequired();

        builder.Property(e => e.OptionNumber)
            .HasColumnName("option_number")
            .IsRequired();

        builder.Property(e => e.MultiplierCode)
            .HasColumnName("multiplier_code")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(e => e.EffectiveDate)
            .HasColumnName("effective_date")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => e.Howident)
            .IsUnique()
            .HasDatabaseName("uq_cdc_pricing_home_option_whitelist_howident");

        builder.Ignore(e => e.DomainEvents);
    }
}
