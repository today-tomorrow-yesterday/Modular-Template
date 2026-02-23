using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Inventory.Domain.LandParcels;

namespace Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class LandParcelConfiguration : IEntityTypeConfiguration<LandParcel>
{
    public void Configure(EntityTypeBuilder<LandParcel> builder)
    {
        builder.ToTable("land_parcels");

        builder.Ignore(l => l.DomainEvents);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(l => l.RefStockNumber)
            .HasColumnName("ref_stock_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.StockType).HasColumnName("stock_type").HasMaxLength(20);
        builder.Property(l => l.LandAge).HasColumnName("land_age").HasMaxLength(20);
        builder.Property(l => l.LandCost).HasColumnName("land_cost");
        builder.Property(l => l.AddToTotal).HasColumnName("add_to_total");
        builder.Property(l => l.Appraisal).HasColumnName("appraisal");
        builder.Property(l => l.MapParcel).HasColumnName("map_parcel").HasMaxLength(100);
        builder.Property(l => l.Address).HasColumnName("address").HasMaxLength(200);
        builder.Property(l => l.Address2).HasColumnName("address2").HasMaxLength(200);
        builder.Property(l => l.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(l => l.State).HasColumnName("state").HasMaxLength(2);
        builder.Property(l => l.Zip).HasColumnName("zip").HasMaxLength(20);
        builder.Property(l => l.County).HasColumnName("county").HasMaxLength(100);
        builder.Property(l => l.LoanNumber).HasColumnName("loan_number").HasMaxLength(100);
        builder.Property(l => l.HomeStockNumber).HasColumnName("home_stock_number").HasMaxLength(50);

        builder.Property(l => l.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(l => new { l.RefHomeCenterNumber, l.RefStockNumber })
            .IsUnique()
            .HasDatabaseName("ix_land_parcels_hc_stock");

        builder.HasIndex(l => l.RefHomeCenterNumber)
            .HasDatabaseName("ix_land_parcels_ref_home_center_number");
    }
}
