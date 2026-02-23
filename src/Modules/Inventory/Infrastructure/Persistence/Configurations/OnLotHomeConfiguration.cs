using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Inventory.Domain.OnLotHomes;

namespace Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class OnLotHomeConfiguration : IEntityTypeConfiguration<OnLotHome>
{
    public void Configure(EntityTypeBuilder<OnLotHome> builder)
    {
        builder.ToTable("on_lot_homes");

        builder.Ignore(h => h.DomainEvents);

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(h => h.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(h => h.RefStockNumber)
            .HasColumnName("ref_stock_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.StockType).HasColumnName("stock_type").HasMaxLength(20);
        builder.Property(h => h.Condition).HasColumnName("condition").HasMaxLength(50);
        builder.Property(h => h.BuildType).HasColumnName("build_type").HasMaxLength(50);
        builder.Property(h => h.Width).HasColumnName("width");
        builder.Property(h => h.Length).HasColumnName("length");
        builder.Property(h => h.NumberOfBedrooms).HasColumnName("number_of_bedrooms");
        builder.Property(h => h.NumberOfBathrooms).HasColumnName("number_of_bathrooms");
        builder.Property(h => h.ModelYear).HasColumnName("model_year");
        builder.Property(h => h.Model).HasColumnName("model").HasMaxLength(200);
        builder.Property(h => h.Make).HasColumnName("make").HasMaxLength(200);
        builder.Property(h => h.Facility).HasColumnName("facility").HasMaxLength(200);
        builder.Property(h => h.SerialNumber).HasColumnName("serial_number").HasMaxLength(100);
        builder.Property(h => h.TotalInvoiceAmount).HasColumnName("total_invoice_amount");
        builder.Property(h => h.PurchaseDiscount).HasColumnName("purchase_discount");
        builder.Property(h => h.OriginalRetailPrice).HasColumnName("original_retail_price");
        builder.Property(h => h.CurrentRetailPrice).HasColumnName("current_retail_price");
        builder.Property(h => h.StockedInDate).HasColumnName("stocked_in_date").HasMaxLength(20);
        builder.Property(h => h.LandStockNumber).HasColumnName("land_stock_number").HasMaxLength(50);

        builder.Property(h => h.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(h => new { h.RefHomeCenterNumber, h.RefStockNumber })
            .IsUnique()
            .HasDatabaseName("ix_on_lot_homes_hc_stock");

        builder.HasIndex(h => h.RefHomeCenterNumber)
            .HasDatabaseName("ix_on_lot_homes_ref_home_center_number");
    }
}
