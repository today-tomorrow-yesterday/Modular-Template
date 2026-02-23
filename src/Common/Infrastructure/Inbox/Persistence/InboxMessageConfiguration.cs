using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rtl.Core.Infrastructure.Inbox.Persistence;

/// <summary>
/// EF Core configuration for InboxMessage entity.
/// </summary>
public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages", "messaging");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Type).IsRequired();
        builder.Property(i => i.Content).IsRequired();
        builder.Property(i => i.OccurredOnUtc).IsRequired();
        builder.Property(i => i.RetryCount).HasDefaultValue(0);
        builder.Property(i => i.NextRetryAtUtc);

        // Index for efficient querying of unprocessed messages eligible for retry
        builder.HasIndex(i => new { i.ProcessedOnUtc, i.NextRetryAtUtc })
            .HasDatabaseName("ix_inbox_messages_processed_next_retry");
    }
}
