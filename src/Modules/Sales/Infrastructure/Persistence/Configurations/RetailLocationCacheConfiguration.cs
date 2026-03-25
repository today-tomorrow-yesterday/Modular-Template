using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.RetailLocationCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class RetailLocationCacheConfiguration : IEntityTypeConfiguration<RetailLocationCache>
{
    public void Configure(EntityTypeBuilder<RetailLocationCache> builder)
    {
        builder.ToTable("retail_location_cache", Schemas.Cache);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(r => r.LocationType)
            .HasColumnName("location_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(r => r.StateCode)
            .HasColumnName("state_code")
            .IsRequired();

        builder.Property(r => r.Zip)
            .HasColumnName("zip")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number");

        builder.Property(r => r.OrganizationMetadata)
            .HasColumnName("organization_metadata")
            .HasColumnType("jsonb");

        builder.Property(r => r.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(r => r.RefHomeCenterNumber)
            .IsUnique()
            .HasDatabaseName("ix_retail_location_cache_ref_home_center_number")
            .HasFilter("ref_home_center_number IS NOT NULL");
    }
}
