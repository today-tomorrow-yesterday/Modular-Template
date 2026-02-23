using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organization.Domain.Regions;

namespace Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("regions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.RefRegionId)
            .HasColumnName("ref_region_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(200);

        builder.Property(r => r.Manager)
            .HasColumnName("manager")
            .HasMaxLength(200);

        builder.Property(r => r.DummyHomeCenterNumber)
            .HasColumnName("dummy_home_center_number");

        builder.Property(r => r.StatusCode)
            .HasColumnName("status_code")
            .HasMaxLength(20);

        builder.Property(r => r.ZoneId)
            .HasColumnName("zone_id");

        builder.Property(r => r.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(r => r.RefRegionId)
            .IsUnique()
            .HasDatabaseName("ix_regions_ref_region_id");

        builder.HasOne(r => r.Zone)
            .WithMany(z => z.Regions)
            .HasForeignKey(r => r.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
