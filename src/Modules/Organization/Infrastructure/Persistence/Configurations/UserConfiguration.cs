using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organization.Domain.Users;

namespace Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.RefUserId)
            .HasColumnName("ref_user_id")
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.MiddleInitial)
            .HasColumnName("middle_initial")
            .HasMaxLength(10);

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(u => u.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.EmailAddress)
            .HasColumnName("email_address")
            .HasMaxLength(300);

        builder.Property(u => u.EmployeeNumber)
            .HasColumnName("employee_number");

        builder.Property(u => u.FederatedId)
            .HasColumnName("federated_id")
            .HasMaxLength(500);

        builder.Property(u => u.DistinguishedName)
            .HasColumnName("distinguished_name")
            .HasMaxLength(500);

        builder.Property(u => u.UserAccountControl)
            .HasColumnName("user_account_control")
            .HasMaxLength(50);

        builder.Property(u => u.Title)
            .HasColumnName("title")
            .HasMaxLength(200);

        builder.Property(u => u.Level1)
            .HasColumnName("level1")
            .HasMaxLength(50);

        builder.Property(u => u.Level2)
            .HasColumnName("level2")
            .HasMaxLength(50);

        builder.Property(u => u.Level3)
            .HasColumnName("level3")
            .HasMaxLength(50);

        builder.Property(u => u.Level4)
            .HasColumnName("level4")
            .HasMaxLength(50);

        builder.Property(u => u.PositionNumber)
            .HasColumnName("position_number")
            .HasMaxLength(50);

        builder.Property(u => u.UserRoles)
            .HasColumnName("user_roles")
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.IsRetired)
            .HasColumnName("is_retired")
            .IsRequired();

        builder.Property(u => u.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(u => u.RefUserId)
            .IsUnique()
            .HasDatabaseName("ix_users_ref_user_id");

        builder.HasIndex(u => u.FederatedId)
            .IsUnique()
            .HasDatabaseName("ix_users_federated_id")
            .HasFilter("federated_id IS NOT NULL");

        builder.HasIndex(u => u.EmployeeNumber)
            .HasDatabaseName("ix_users_employee_number")
            .HasFilter("employee_number IS NOT NULL");
    }
}
