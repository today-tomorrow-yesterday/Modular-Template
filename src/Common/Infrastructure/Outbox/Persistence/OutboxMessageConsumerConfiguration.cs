using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rtl.Core.Infrastructure.Outbox.Persistence;

/// <summary>
/// EF Core configuration for OutboxMessageConsumer.
/// </summary>
public sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
    {
        builder.ToTable("outbox_message_consumers", "messaging");

        builder.HasKey(o => new { o.OutboxMessageId, o.Name });

        builder.Property(o => o.Name).HasMaxLength(500);
    }
}
