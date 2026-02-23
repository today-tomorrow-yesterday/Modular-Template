using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleSales.Domain.Catalogs;

namespace Modules.SampleSales.Infrastructure.Persistence.Configurations;

internal sealed class CatalogProductConfiguration : IEntityTypeConfiguration<CatalogProduct>
{
    public void Configure(EntityTypeBuilder<CatalogProduct> builder)
    {
        builder.ToTable("catalog_products");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Id)
            .HasColumnName("id");

        builder.Property(cp => cp.CatalogId)
            .HasColumnName("catalog_id")
            .IsRequired();

        builder.Property(cp => cp.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(cp => cp.AddedAtUtc)
            .HasColumnName("added_at_utc")
            .IsRequired();

        // Configure optional CustomPrice Money value object
        builder.OwnsOne(cp => cp.CustomPrice, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("custom_price_amount")
                .HasPrecision(18, 2);

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("custom_price_currency")
                .HasMaxLength(3);
        });

        // Unique constraint on (CatalogId, ProductId)
        builder.HasIndex(cp => new { cp.CatalogId, cp.ProductId }).IsUnique();

        builder.HasIndex(cp => cp.ProductId);
    }
}
