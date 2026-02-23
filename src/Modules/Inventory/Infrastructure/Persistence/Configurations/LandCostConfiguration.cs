using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Inventory.Domain.LandCosts;

namespace Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class LandCostConfiguration : IEntityTypeConfiguration<LandCost>
{
    public void Configure(EntityTypeBuilder<LandCost> builder)
    {
        builder.ToTable("land_costs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(l => l.RefStockNumber)
            .HasColumnName("ref_stock_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.AddToTotal).HasColumnName("add_to_total");
        builder.Property(l => l.FurnitureTotal).HasColumnName("furniture_total");

        builder.Property(l => l.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(l => new { l.RefHomeCenterNumber, l.RefStockNumber })
            .IsUnique()
            .HasDatabaseName("ix_land_costs_hc_stock");
    }
}
