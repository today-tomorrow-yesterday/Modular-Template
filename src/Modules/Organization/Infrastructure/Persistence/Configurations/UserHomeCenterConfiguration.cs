using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organization.Domain.Users;

namespace Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class UserHomeCenterConfiguration : IEntityTypeConfiguration<UserHomeCenter>
{
    public void Configure(EntityTypeBuilder<UserHomeCenter> builder)
    {
        builder.ToTable("user_home_centers");

        builder.HasKey(uhc => uhc.Id);

        builder.Property(uhc => uhc.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(uhc => uhc.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(uhc => uhc.HomeCenterId)
            .HasColumnName("home_center_id")
            .IsRequired();

        builder.Property(uhc => uhc.AssignmentType)
            .HasColumnName("assignment_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(uhc => uhc.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.Property(uhc => uhc.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(uhc => new { uhc.UserId, uhc.HomeCenterId })
            .IsUnique()
            .HasDatabaseName("ix_user_home_centers_user_id_home_center_id");

        builder.HasIndex(uhc => uhc.HomeCenterId)
            .HasDatabaseName("ix_user_home_centers_home_center_id");

        builder.HasOne(uhc => uhc.User)
            .WithMany(u => u.UserHomeCenters)
            .HasForeignKey(uhc => uhc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(uhc => uhc.HomeCenter)
            .WithMany(hc => hc.UserHomeCenters)
            .HasForeignKey(uhc => uhc.HomeCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
