using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class SalesAssignmentConfiguration : IEntityTypeConfiguration<SalesAssignment>
{
    public void Configure(EntityTypeBuilder<SalesAssignment> builder)
    {
        builder.ToTable("sales_assignments");

        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(sa => sa.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(sa => sa.SalesPersonId)
            .HasColumnName("sales_person_id")
            .HasMaxLength(200);

        builder.Property(sa => sa.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(50);

        // Exactly one Primary per Customer; multiple Supporting allowed
        builder.HasIndex(sa => sa.CustomerId)
            .IsUnique()
            .HasFilter($"role = '{nameof(SalesAssignmentRole.Primary)}'");

        // Prevent same SalesPerson assigned twice to the same Customer
        builder.HasIndex(sa => new { sa.CustomerId, sa.SalesPersonId })
            .IsUnique();

        builder.HasOne(sa => sa.SalesPerson)
            .WithMany()
            .HasForeignKey(sa => sa.SalesPersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
