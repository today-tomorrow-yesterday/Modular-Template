using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleOrders.Domain.ProductsCache;

namespace Modules.SampleOrders.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ProductCache entity.
/// Note: No audit or soft delete fields - cache entities are simple data copies.
/// </summary>
internal sealed class ProductCacheConfiguration : IEntityTypeConfiguration<ProductCache>
{
    public void Configure(EntityTypeBuilder<ProductCache> builder)
    {
        builder.ToTable("products_cache", "cache");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(p => p.IsActive);
    }
}
