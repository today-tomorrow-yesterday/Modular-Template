using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rtl.Core.Infrastructure.Outbox.Persistence;

/// <summary>
/// EF Core configuration for OutboxMessage entity.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type).IsRequired();
        builder.Property(o => o.Content).IsRequired();
        builder.Property(o => o.OccurredOnUtc).IsRequired();
        builder.Property(o => o.RetryCount).HasDefaultValue(0);
        builder.Property(o => o.NextRetryAtUtc);

        // Index for efficient querying of unprocessed messages eligible for retry
        builder.HasIndex(o => new { o.ProcessedOnUtc, o.NextRetryAtUtc })
            .HasDatabaseName("ix_outbox_messages_processed_next_retry");
    }
}
