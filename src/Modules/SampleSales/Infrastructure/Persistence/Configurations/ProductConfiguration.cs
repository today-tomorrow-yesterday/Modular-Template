using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleSales.Domain.Products;
using ModularTemplate.Infrastructure.Auditing.Configurations;

namespace Modules.SampleSales.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.PublicId)
            .HasColumnName("public_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(p => p.InternalCost)
            .HasColumnName("internal_cost")
            .HasPrecision(18, 2);

        // Configure Price Money value object
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("price_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(p => p.Price).IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasIndex(p => p.PublicId)
            .IsUnique()
            .HasDatabaseName("ix_products_public_id");

        // Configure audit fields from IAuditableEntity
        builder.ConfigureAuditProperties();

        // Configure soft delete fields from ISoftDeletable
        builder.ConfigureSoftDeleteProperties();
    }
}
