using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.DeliveryAddresses;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class DeliveryAddressConfiguration : IEntityTypeConfiguration<DeliveryAddress>
{
    public void Configure(EntityTypeBuilder<DeliveryAddress> builder)
    {
        builder.ToTable("delivery_addresses");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id");

        builder.Property(d => d.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.SaleId)
            .HasColumnName("sale_id")
            .IsRequired();

        builder.Property(d => d.AddressStyle)
            .HasColumnName("address_style");

        builder.Property(d => d.AddressType)
            .HasColumnName("address_type");

        builder.Property(d => d.AddressLine1)
            .HasColumnName("address_line_1");

        builder.Property(d => d.AddressLine2)
            .HasColumnName("address_line_2");

        builder.Property(d => d.AddressLine3)
            .HasColumnName("address_line_3");

        builder.Property(d => d.City)
            .HasColumnName("city");

        builder.Property(d => d.County)
            .HasColumnName("county");

        builder.Property(d => d.State)
            .HasColumnName("state");

        builder.Property(d => d.Country)
            .HasColumnName("country");

        builder.Property(d => d.PostalCode)
            .HasColumnName("postal_code");

        builder.Property(d => d.OccupancyType)
            .HasColumnName("occupancy_type");

        builder.Property(d => d.IsWithinCityLimits)
            .HasColumnName("is_within_city_limits");

        builder.ConfigureAuditProperties();

        builder.HasIndex(d => d.PublicId)
            .IsUnique()
            .HasDatabaseName("ix_delivery_addresses_public_id");

        builder.HasIndex(d => d.SaleId)
            .IsUnique()
            .HasDatabaseName("ix_delivery_addresses_sale_id");

        builder.HasOne(d => d.Sale)
            .WithOne(s => s.DeliveryAddress)
            .HasForeignKey<DeliveryAddress>(d => d.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
