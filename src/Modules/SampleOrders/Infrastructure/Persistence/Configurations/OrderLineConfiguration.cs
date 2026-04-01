using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.Orders;
using ModularTemplate.Infrastructure.Persistence.Versioning;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        // TPH discriminator — single table, type determined by line_type column
        builder.HasDiscriminator<string>("line_type")
            .HasValue<ProductLine>("Product")
            .HasValue<CustomLine>("Custom");

        builder.Property(l => l.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(l => l.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(l => l.SortOrder)
            .HasColumnName("sort_order")
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

        builder.HasIndex(l => l.OrderId)
            .HasDatabaseName("ix_order_lines_order_id");
    }
}

internal sealed class ProductLineConfiguration : IEntityTypeConfiguration<ProductLine>
{
    public void Configure(EntityTypeBuilder<ProductLine> builder)
    {
        builder.Property(l => l.ProductCacheId)
            .HasColumnName("product_cache_id");

        // Single JSONB 'details' column — VersionedJsonConverter handles schema evolution
        builder.Property(l => l.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<ProductLineDetails>());

        builder.HasIndex(l => l.ProductCacheId)
            .HasDatabaseName("ix_order_lines_product_cache_id");
    }
}

internal sealed class CustomLineConfiguration : IEntityTypeConfiguration<CustomLine>
{
    public void Configure(EntityTypeBuilder<CustomLine> builder)
    {
        // Same JSONB 'details' column — different Details type per line type
        builder.Property(l => l.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<CustomLineDetails>());
    }
}
