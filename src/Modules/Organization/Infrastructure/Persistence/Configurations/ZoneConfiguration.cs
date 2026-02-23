using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organization.Domain.Zones;

namespace Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zones");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(z => z.RefZoneId)
            .HasColumnName("ref_zone_id")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(z => z.Manager)
            .HasColumnName("manager")
            .HasMaxLength(200);

        builder.Property(z => z.StatusCode)
            .HasColumnName("status_code")
            .HasMaxLength(20);

        builder.Property(z => z.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(z => z.RefZoneId)
            .IsUnique()
            .HasDatabaseName("ix_zones_ref_zone_id");
    }
}
