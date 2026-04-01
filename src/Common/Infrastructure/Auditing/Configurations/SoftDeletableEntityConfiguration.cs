using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularTemplate.Domain.Entities;

namespace ModularTemplate.Infrastructure.Auditing.Configurations;

/// <summary>
/// Extension methods for configuring soft-deletable entity properties in EF Core.
/// </summary>
public static class SoftDeletableEntityConfiguration
{
    /// <summary>
    /// Configures the soft delete properties for an entity that implements ISoftDeletable.
    /// Call this in your entity configuration after ConfigureAuditProperties().
    /// </summary>
    /// <remarks>
    /// This method:
    /// - Maps the soft delete properties to snake_case column names
    /// - Adds a global query filter to exclude soft-deleted entities by default
    /// - Creates an index on IsDeleted for query performance
    /// </remarks>
    public static void ConfigureSoftDeleteProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ISoftDeletable
    {
        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        builder.Property(e => e.DeletedByUserId)
            .HasColumnName("deleted_by_user_id");

        // Global query filter to exclude soft-deleted entities by default
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Create index on IsDeleted for query performance
        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName($"ix_{typeof(TEntity).Name.ToLowerInvariant()}_is_deleted");
    }
}
