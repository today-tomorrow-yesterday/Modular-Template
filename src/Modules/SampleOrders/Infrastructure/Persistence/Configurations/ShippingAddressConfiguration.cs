using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.Orders;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

internal sealed class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddress>
{
    public void Configure(EntityTypeBuilder<ShippingAddress> builder)
    {
        builder.ToTable("shipping_addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.OwnsOne(a => a.Address, addressBuilder =>
        {
            addressBuilder.Property(addr => addr.AddressLine1).HasColumnName("address_line1").HasMaxLength(200).IsRequired();
            addressBuilder.Property(addr => addr.AddressLine2).HasColumnName("address_line2").HasMaxLength(200);
            addressBuilder.Property(addr => addr.City).HasColumnName("city").HasMaxLength(100).IsRequired();
            addressBuilder.Property(addr => addr.State).HasColumnName("state").HasMaxLength(2).IsRequired();
            addressBuilder.Property(addr => addr.PostalCode).HasColumnName("postal_code").HasMaxLength(20).IsRequired();
            addressBuilder.Property(addr => addr.Country).HasColumnName("country").HasMaxLength(3);
        });

        builder.Navigation(a => a.Address).IsRequired();

        builder.HasIndex(a => a.OrderId)
            .IsUnique()
            .HasDatabaseName("ix_shipping_addresses_order_id");
    }
}
