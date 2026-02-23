using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.Orders;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(l => l.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(l => l.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        // Configure UnitPrice Money value object
        builder.OwnsOne(l => l.UnitPrice, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("unit_price_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("unit_price_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(l => l.UnitPrice).IsRequired();

        // Ignore computed LineTotal property
        builder.Ignore(l => l.LineTotal);

        builder.HasIndex(l => l.OrderId);
        builder.HasIndex(l => l.ProductId);
    }
}
