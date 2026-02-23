using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.Orders;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.OrderedAtUtc)
            .HasColumnName("ordered_at_utc")
            .IsRequired();

        // Configure Lines collection
        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed TotalPrice property - calculated from Lines
        builder.Ignore(o => o.TotalPrice);

        // Configure audit fields from IAuditableEntity
        builder.ConfigureAuditProperties();

        // Configure soft delete fields from ISoftDeletable
        builder.ConfigureSoftDeleteProperties();

        builder.HasIndex(o => o.CustomerId);
    }
}
