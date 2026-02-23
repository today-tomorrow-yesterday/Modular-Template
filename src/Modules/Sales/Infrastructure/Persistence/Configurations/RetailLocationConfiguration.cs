using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.RetailLocations;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class RetailLocationConfiguration : IEntityTypeConfiguration<RetailLocation>
{
    public void Configure(EntityTypeBuilder<RetailLocation> builder)
    {
        builder.ToTable("retail_locations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

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

        builder.ConfigureAuditProperties();

        builder.HasIndex(r => r.RefHomeCenterNumber)
            .IsUnique()
            .HasDatabaseName("ix_retail_locations_ref_home_center_number")
            .HasFilter("ref_home_center_number IS NOT NULL");

        builder.Ignore(r => r.DomainEvents);
    }
}
