using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.Customers;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

internal sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("customer_addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.OwnsOne(a => a.Address, addressBuilder =>
        {
            addressBuilder.Property(addr => addr.AddressLine1).HasColumnName("address_line1").HasMaxLength(200);
            addressBuilder.Property(addr => addr.AddressLine2).HasColumnName("address_line2").HasMaxLength(200);
            addressBuilder.Property(addr => addr.City).HasColumnName("city").HasMaxLength(100);
            addressBuilder.Property(addr => addr.State).HasColumnName("state").HasMaxLength(2);
            addressBuilder.Property(addr => addr.PostalCode).HasColumnName("postal_code").HasMaxLength(20);
            addressBuilder.Property(addr => addr.Country).HasColumnName("country").HasMaxLength(3);
        });

        builder.Navigation(a => a.Address).IsRequired();

        builder.Property(a => a.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.HasIndex(a => a.CustomerId)
            .HasDatabaseName("ix_customer_addresses_customer_id");
    }
}
