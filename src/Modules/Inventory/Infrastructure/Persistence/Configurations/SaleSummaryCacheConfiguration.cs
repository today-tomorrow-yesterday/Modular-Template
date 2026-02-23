using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Inventory.Domain.SaleSummariesCache;

namespace Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class SaleSummaryCacheConfiguration : IEntityTypeConfiguration<SaleSummaryCache>
{
    public void Configure(EntityTypeBuilder<SaleSummaryCache> builder)
    {
        builder.ToTable("sale_summaries_cache", "cache");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.RefStockNumber)
            .HasColumnName("ref_stock_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.SaleId).HasColumnName("sale_id");
        builder.Property(s => s.CustomerName).HasColumnName("customer_name").HasMaxLength(300);
        builder.Property(s => s.ReceivedInDate).HasColumnName("received_in_date");
        builder.Property(s => s.OriginalRetailPrice).HasColumnName("original_retail_price");
        builder.Property(s => s.CurrentRetailPrice).HasColumnName("current_retail_price");

        builder.Property(s => s.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(s => s.RefStockNumber)
            .IsUnique()
            .HasDatabaseName("ix_sale_summaries_cache_ref_stock_number");
    }
}
