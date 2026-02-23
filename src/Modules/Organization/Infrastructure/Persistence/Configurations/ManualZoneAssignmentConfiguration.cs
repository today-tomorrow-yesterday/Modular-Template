using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organization.Domain.ManualAssignments;

namespace Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class ManualZoneAssignmentConfiguration : IEntityTypeConfiguration<ManualZoneAssignment>
{
    public void Configure(EntityTypeBuilder<ManualZoneAssignment> builder)
    {
        builder.ToTable("manual_zone_assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.RefAssignmentId)
            .HasColumnName("ref_assignment_id")
            .IsRequired();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.Zone)
            .HasColumnName("zone")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(a => a.StatusCode)
            .HasColumnName("status_code")
            .HasMaxLength(20);

        builder.Property(a => a.Manager)
            .HasColumnName("manager")
            .HasMaxLength(200);

        builder.Property(a => a.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(a => a.RefAssignmentId)
            .IsUnique()
            .HasDatabaseName("ix_manual_zone_assignments_ref_assignment_id");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_manual_zone_assignments_user_id");

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
