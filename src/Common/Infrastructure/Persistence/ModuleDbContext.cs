using Microsoft.EntityFrameworkCore;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;
using Rtl.Core.Infrastructure.Auditing.Configurations;
using Rtl.Core.Infrastructure.Inbox.Persistence;
using Rtl.Core.Infrastructure.Outbox.Persistence;
using Rtl.Core.Infrastructure.Security;
using System.Reflection;

namespace Rtl.Core.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for all modules, providing common infrastructure configurations.
/// </summary>
public abstract class ModuleDbContext<TContext>(DbContextOptions<TContext> options)
    : DbContext(options), IUnitOfWork
    where TContext : DbContext
{
    protected abstract string Schema { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        // Configure Hi-Lo sequence for all Entity-derived types.
        // This ensures IDs are assigned at DbContext.Add() time (before SaveChanges),
        // which is critical for the outbox pattern to enrich domain events with entity IDs.
        var sequenceName = $"{Schema}_hilo_seq";
        modelBuilder.HasSequence<int>(sequenceName, Schema)
            .StartsAt(1)
            .IncrementsBy(10);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(Entity.Id))
                    .UseHiLo(sequenceName, Schema);
            }
        }

        // Outbox pattern configurations
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());

        // Inbox pattern configurations
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());

        // Audit trail configuration
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // Apply Encryption to [SensitiveData] properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.PropertyInfo?.GetCustomAttribute<SensitiveDataAttribute>() is null)
                    continue;

                if (property.ClrType == typeof(string))
                {
                    property.SetValueConverter(typeof(EncryptionValueConverter));
                }
                else
                {
                    // Use generic JSON converter for other types
                    var converterType = typeof(JsonEncryptionValueConverter<>).MakeGenericType(property.ClrType);
                    property.SetValueConverter(converterType);
                }
            }
        }
    }
}
