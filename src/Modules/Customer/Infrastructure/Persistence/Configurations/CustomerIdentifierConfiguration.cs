using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Customers.Entities;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class CustomerIdentifierConfiguration : IEntityTypeConfiguration<CustomerIdentifier>
{
    public void Configure(EntityTypeBuilder<CustomerIdentifier> builder)
    {
        builder.ToTable("customer_identifiers");

        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.Id).HasColumnName("id");
        builder.Property(ci => ci.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(ci => ci.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(ci => ci.Value).HasColumnName("value").HasMaxLength(200).IsRequired();

        builder.HasIndex(ci => new { ci.CustomerId, ci.Type })
            .IsUnique()
            .HasDatabaseName("uq_customer_identifiers_customer_id_type");

        builder.HasIndex(ci => new { ci.Type, ci.Value })
            .HasDatabaseName("ix_customer_identifiers_type_value");

        builder.Ignore(ci => ci.DomainEvents);
    }
}
