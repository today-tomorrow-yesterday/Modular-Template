using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Inventory.Domain.HomeCentersCache;

namespace Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class HomeCenterCacheConfiguration : IEntityTypeConfiguration<HomeCenterCache>
{
    public void Configure(EntityTypeBuilder<HomeCenterCache> builder)
    {
        builder.ToTable("home_centers_cache", "cache");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(h => h.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(h => h.LotName)
            .HasColumnName("lot_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.StateCode).HasColumnName("state_code").HasMaxLength(2);
        builder.Property(h => h.ZoneId).HasColumnName("zone_id");
        builder.Property(h => h.RegionId).HasColumnName("region_id");
        builder.Property(h => h.Latitude).HasColumnName("latitude");
        builder.Property(h => h.Longitude).HasColumnName("longitude");

        builder.Property(h => h.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(h => h.RefHomeCenterNumber)
            .IsUnique()
            .HasDatabaseName("ix_home_centers_cache_ref_home_center_number");
    }
}
