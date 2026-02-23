using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.InventoryCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class OnLotHomeCacheConfiguration : IEntityTypeConfiguration<OnLotHomeCache>
{
    public void Configure(EntityTypeBuilder<OnLotHomeCache> builder)
    {
        builder.ToTable("on_lot_homes_cache", Schemas.Cache);

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(h => h.RefOnLotHomeId)
            .HasColumnName("ref_on_lot_home_id")
            .IsRequired();

        builder.Property(h => h.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(h => h.RefStockNumber)
            .HasColumnName("ref_stock_number")
            .IsRequired();

        builder.Property(h => h.StockType).HasColumnName("stock_type");
        builder.Property(h => h.Condition).HasColumnName("condition").HasConversion<string>();
        builder.Property(h => h.BuildType).HasColumnName("build_type");
        builder.Property(h => h.Width).HasColumnName("width");
        builder.Property(h => h.Length).HasColumnName("length");
        builder.Property(h => h.NumberOfBedrooms).HasColumnName("number_of_bedrooms");
        builder.Property(h => h.NumberOfBathrooms).HasColumnName("number_of_bathrooms");
        builder.Property(h => h.ModelYear).HasColumnName("model_year");
        builder.Property(h => h.Model).HasColumnName("model");
        builder.Property(h => h.Make).HasColumnName("make");
        builder.Property(h => h.Facility).HasColumnName("facility");
        builder.Property(h => h.SerialNumber).HasColumnName("serial_number");
        builder.Property(h => h.TotalInvoiceAmount).HasColumnName("total_invoice_amount");
        builder.Property(h => h.OriginalRetailPrice).HasColumnName("original_retail_price");
        builder.Property(h => h.CurrentRetailPrice).HasColumnName("current_retail_price");

        builder.Property(h => h.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        // Unique constraints
        builder.HasIndex(h => h.RefOnLotHomeId)
            .IsUnique()
            .HasDatabaseName("ix_on_lot_homes_cache_ref_on_lot_home_id");

        builder.HasIndex(h => new { h.RefHomeCenterNumber, h.RefStockNumber })
            .IsUnique()
            .HasDatabaseName("ix_on_lot_homes_cache_hc_stock");
    }
}
