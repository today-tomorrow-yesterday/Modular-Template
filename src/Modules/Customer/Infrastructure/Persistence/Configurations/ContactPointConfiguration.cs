using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Customers.Entities;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class ContactPointConfiguration : IEntityTypeConfiguration<ContactPoint>
{
    public void Configure(EntityTypeBuilder<ContactPoint> builder)
    {
        builder.ToTable("contact_points");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Id)
            .HasColumnName("id");

        builder.Property(cp => cp.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(cp => cp.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cp => cp.Value)
            .HasColumnName("value")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cp => cp.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.HasIndex(cp => new { cp.CustomerId, cp.Type })
            .HasDatabaseName("ix_contact_points_customer_id_type");

        builder.Ignore(cp => cp.DomainEvents);
    }
}
