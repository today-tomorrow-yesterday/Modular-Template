using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rtl.Core.Infrastructure.Auditing.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.EntityName)
            .HasColumnName("entity_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .IsRequired();

        builder.Property(x => x.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(x => x.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(x => x.AffectedColumns)
            .HasColumnName("affected_columns")
            .HasColumnType("jsonb");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(256);

        builder.Property(x => x.TimestampUtc)
            .HasColumnName("timestamp_utc")
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(256);

        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasMaxLength(256);

        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(512);

        // Indexes
        builder.HasIndex(x => new { x.EntityName, x.EntityId })
            .HasDatabaseName("ix_audit_logs_entity");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_audit_logs_user");

        builder.HasIndex(x => x.TimestampUtc)
            .HasDatabaseName("ix_audit_logs_timestamp");

        builder.HasIndex(x => x.CorrelationId)
            .HasDatabaseName("ix_audit_logs_correlation");
    }
}
