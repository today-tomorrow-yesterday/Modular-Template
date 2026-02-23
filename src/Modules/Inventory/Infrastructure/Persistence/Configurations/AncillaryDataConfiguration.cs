using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class AncillaryDataConfiguration : IEntityTypeConfiguration<Domain.AncillaryData.AncillaryData>
{
    public void Configure(EntityTypeBuilder<Domain.AncillaryData.AncillaryData> builder)
    {
        builder.ToTable("ancillary_data");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(a => a.RefStockNumber)
            .HasColumnName("ref_stock_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(300);

        builder.Property(a => a.PackageReceivedDate)
            .HasColumnName("package_received_date");

        builder.Property(a => a.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(a => new { a.RefHomeCenterNumber, a.RefStockNumber })
            .IsUnique()
            .HasDatabaseName("ix_ancillary_data_hc_stock");
    }
}
