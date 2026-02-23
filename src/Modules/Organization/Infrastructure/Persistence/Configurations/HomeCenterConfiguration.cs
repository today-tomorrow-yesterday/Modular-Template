using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Organization.Domain.HomeCenters;

namespace Modules.Organization.Infrastructure.Persistence.Configurations;

internal sealed class HomeCenterConfiguration : IEntityTypeConfiguration<HomeCenter>
{
    public void Configure(EntityTypeBuilder<HomeCenter> builder)
    {
        builder.ToTable("home_centers");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(h => h.RefHomeCenterNumber)
            .HasColumnName("ref_home_center_number")
            .IsRequired();

        builder.Property(h => h.LotMdlr)
            .HasColumnName("lot_mdlr")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.LotName)
            .HasColumnName("lot_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.LotDba)
            .HasColumnName("lot_dba")
            .HasMaxLength(200);

        builder.Property(h => h.Brand)
            .HasColumnName("brand")
            .HasMaxLength(100);

        builder.Property(h => h.LotStatus)
            .HasColumnName("lot_status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.Address1)
            .HasColumnName("address1")
            .HasMaxLength(200);

        builder.Property(h => h.Address2)
            .HasColumnName("address2")
            .HasMaxLength(200);

        builder.Property(h => h.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(h => h.StateCode)
            .HasColumnName("state_code")
            .HasMaxLength(2);

        builder.Property(h => h.Zip)
            .HasColumnName("zip")
            .HasMaxLength(20);

        builder.Property(h => h.MailingAddress1)
            .HasColumnName("mailing_address1")
            .HasMaxLength(200);

        builder.Property(h => h.MailingAddress2)
            .HasColumnName("mailing_address2")
            .HasMaxLength(200);

        builder.Property(h => h.MailingCity)
            .HasColumnName("mailing_city")
            .HasMaxLength(100);

        builder.Property(h => h.MailingStateCode)
            .HasColumnName("mailing_state_code")
            .HasMaxLength(2);

        builder.Property(h => h.MailingZip)
            .HasColumnName("mailing_zip")
            .HasMaxLength(20);

        builder.Property(h => h.ZoneId)
            .HasColumnName("zone_id");

        builder.Property(h => h.RegionId)
            .HasColumnName("region_id");

        builder.Property(h => h.Latitude)
            .HasColumnName("latitude");

        builder.Property(h => h.Longitude)
            .HasColumnName("longitude");

        builder.Property(h => h.AreaCode)
            .HasColumnName("area_code")
            .HasMaxLength(10);

        builder.Property(h => h.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(h => h.ManagerEmployeeNumber)
            .HasColumnName("manager_employee_number");

        builder.Property(h => h.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(h => h.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(h => h.RefHomeCenterNumber)
            .IsUnique()
            .HasDatabaseName("ix_home_centers_ref_home_center_number");

        builder.HasOne(h => h.Zone)
            .WithMany(z => z.HomeCenters)
            .HasForeignKey(h => h.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Region)
            .WithMany(r => r.HomeCenters)
            .HasForeignKey(h => h.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
