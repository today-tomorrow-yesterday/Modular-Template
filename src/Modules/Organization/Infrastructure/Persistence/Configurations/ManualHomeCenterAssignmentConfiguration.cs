using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organization.Domain.ManualAssignments;

namespace Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class ManualHomeCenterAssignmentConfiguration : IEntityTypeConfiguration<ManualHomeCenterAssignment>
{
    public void Configure(EntityTypeBuilder<ManualHomeCenterAssignment> builder)
    {
        builder.ToTable("manual_hc_assignments");

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

        builder.Property(a => a.HomeCenterId)
            .HasColumnName("home_center_id")
            .IsRequired();

        builder.Property(a => a.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(a => a.RefAssignmentId)
            .IsUnique()
            .HasDatabaseName("ix_manual_hc_assignments_ref_assignment_id");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_manual_hc_assignments_user_id");

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.HomeCenter)
            .WithMany()
            .HasForeignKey(a => a.HomeCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
