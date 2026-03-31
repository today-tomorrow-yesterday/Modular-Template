using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.Customers;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

internal sealed class CustomerContactConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> builder)
    {
        builder.ToTable("customer_contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Value)
            .HasColumnName("value")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.HasIndex(c => new { c.CustomerId, c.Type, c.Value })
            .IsUnique()
            .HasDatabaseName("ix_customer_contacts_customer_type_value");
    }
}
