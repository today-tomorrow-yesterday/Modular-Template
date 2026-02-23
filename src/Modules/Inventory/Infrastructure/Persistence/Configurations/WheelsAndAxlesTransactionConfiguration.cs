using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Inventory.Domain.WheelsAndAxles;

namespace Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class WheelsAndAxlesTransactionConfiguration : IEntityTypeConfiguration<WheelsAndAxlesTransaction>
{
    public void Configure(EntityTypeBuilder<WheelsAndAxlesTransaction> builder)
    {
        builder.ToTable("wheels_and_axles_transactions");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(w => w.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(w => w.RefTransactionId)
            .HasColumnName("ref_transaction_id")
            .IsRequired();

        builder.Property(w => w.Date).HasColumnName("date");
        builder.Property(w => w.Type).HasColumnName("type").HasMaxLength(50);
        builder.Property(w => w.StockNumber).HasColumnName("stock_number").HasMaxLength(50);
        builder.Property(w => w.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(w => w.Wheels).HasColumnName("wheels");
        builder.Property(w => w.WheelValue).HasColumnName("wheel_value");
        builder.Property(w => w.BrakeAxles).HasColumnName("brake_axles");
        builder.Property(w => w.BrakeAxleValue).HasColumnName("brake_axle_value");
        builder.Property(w => w.IdlerAxles).HasColumnName("idler_axles");
        builder.Property(w => w.IdlerAxleValue).HasColumnName("idler_axle_value");
        builder.Property(w => w.TotalWheelsAndAxlesValue).HasColumnName("total_wheels_and_axles_value");

        builder.Property(w => w.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(w => new { w.RefHomeCenterNumber, w.RefTransactionId })
            .IsUnique()
            .HasDatabaseName("ix_wheels_and_axles_hc_txn");

        builder.HasIndex(w => w.RefHomeCenterNumber)
            .HasDatabaseName("ix_wheels_and_axles_ref_home_center_number");
    }
}
