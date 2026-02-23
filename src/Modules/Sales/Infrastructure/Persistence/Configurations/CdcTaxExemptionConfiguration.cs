using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcTaxExemptionConfiguration : IEntityTypeConfiguration<CdcTaxExemption>
{
    public void Configure(EntityTypeBuilder<CdcTaxExemption> builder)
    {
        builder.ToTable("tax_exemption", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ExemptionCode)
            .HasColumnName("exemption_code")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description");

        builder.Property(e => e.RulesText)
            .HasColumnName("rules_text");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => e.ExemptionCode)
            .IsUnique()
            .HasDatabaseName("uq_cdc_tax_exemption_code");

        builder.Ignore(e => e.DomainEvents);
    }
}
