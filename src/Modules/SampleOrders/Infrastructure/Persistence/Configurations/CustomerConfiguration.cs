using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.Customers;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // Configure Email value object
        builder.OwnsOne(c => c.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            emailBuilder.HasIndex(e => e.Value).IsUnique();
        });

        builder.Navigation(c => c.Email).IsRequired();

        // Configure audit fields from IAuditableEntity
        builder.ConfigureAuditProperties();

        // Configure soft delete fields from ISoftDeletable
        builder.ConfigureSoftDeleteProperties();
    }
}
