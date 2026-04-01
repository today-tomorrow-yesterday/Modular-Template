using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularTemplate.Domain.Entities;

namespace ModularTemplate.Infrastructure.Auditing.Configurations;

/// <summary>
/// Extension methods for configuring auditable entity properties in EF Core.
/// </summary>
public static class AuditableEntityConfiguration
{
    /// <summary>
    /// Configures the audit properties for an entity that implements IAuditableEntity.
    /// Call this in your entity configuration to add standard audit field mappings.
    /// </summary>
    public static void ConfigureAuditProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditableEntity
    {
        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.Property(e => e.ModifiedByUserId)
            .HasColumnName("modified_by_user_id");
    }
}
