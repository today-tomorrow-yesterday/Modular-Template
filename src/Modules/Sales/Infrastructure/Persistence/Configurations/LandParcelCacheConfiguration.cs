using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.InventoryCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class LandParcelCacheConfiguration : IEntityTypeConfiguration<LandParcelCache>
{
    public void Configure(EntityTypeBuilder<LandParcelCache> builder)
    {
        builder.ToTable("land_parcels_cache", Schemas.Cache);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(l => l.RefLandParcelId)
            .HasColumnName("ref_land_parcel_id")
            .IsRequired();

        builder.Property(l => l.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(l => l.RefStockNumber)
            .HasColumnName("ref_stock_number")
            .IsRequired();

        builder.Property(l => l.StockType).HasColumnName("stock_type");
        builder.Property(l => l.LandCost).HasColumnName("land_cost");
        builder.Property(l => l.Appraisal).HasColumnName("appraisal");
        builder.Property(l => l.Address).HasColumnName("address");
        builder.Property(l => l.City).HasColumnName("city");
        builder.Property(l => l.State).HasColumnName("state");
        builder.Property(l => l.Zip).HasColumnName("zip");
        builder.Property(l => l.County).HasColumnName("county");

        builder.Property(l => l.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        // Unique constraints
        builder.HasIndex(l => l.RefLandParcelId)
            .IsUnique()
            .HasDatabaseName("ix_land_parcels_cache_ref_land_parcel_id");

        builder.HasIndex(l => new { l.RefHomeCenterNumber, l.RefStockNumber })
            .IsUnique()
            .HasDatabaseName("ix_land_parcels_cache_hc_stock");
    }
}
