using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.AuthorizedUsersCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class AuthorizedUserCacheConfiguration : IEntityTypeConfiguration<AuthorizedUserCache>
{
    public void Configure(EntityTypeBuilder<AuthorizedUserCache> builder)
    {
        builder.ToTable("authorized_users_cache", Schemas.Cache);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.RefUserId)
            .HasColumnName("ref_user_id")
            .IsRequired();

        builder.Property(u => u.FederatedId)
            .HasColumnName("federated_id");

        builder.Property(u => u.EmployeeNumber)
            .HasColumnName("employee_number");

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .IsRequired();

        builder.Property(u => u.EmailAddress)
            .HasColumnName("email_address");

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.IsRetired)
            .HasColumnName("is_retired")
            .IsRequired();

        builder.Property(u => u.AuthorizedHomeCenters)
            .HasColumnName("authorized_home_centers")
            .IsRequired();

        builder.Property(u => u.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(u => u.RefUserId)
            .IsUnique()
            .HasDatabaseName("ix_authorized_users_cache_ref_user_id");

        builder.HasIndex(u => u.FederatedId)
            .IsUnique()
            .HasDatabaseName("ix_authorized_users_cache_federated_id")
            .HasFilter("federated_id IS NOT NULL");

        builder.HasIndex(u => u.EmployeeNumber)
            .IsUnique()
            .HasDatabaseName("uq_authorized_users_cache_employee_number");
    }
}
