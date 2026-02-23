using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.PartiesCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class PartyCacheConfiguration : IEntityTypeConfiguration<PartyCache>
{
    public void Configure(EntityTypeBuilder<PartyCache> builder)
    {
        builder.ToTable("parties", Schemas.Cache);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(p => p.RefPartyId)
            .HasColumnName("ref_party_id")
            .IsRequired();

        builder.Property(p => p.RefPublicId)
            .HasColumnName("ref_public_id")
            .IsRequired();

        builder.Property(p => p.PartyType)
            .HasColumnName("party_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.LifecycleStage)
            .HasColumnName("lifecycle_stage")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.HomeCenterNumber)
            .HasColumnName("home_center_number")
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasColumnName("display_name")
            .IsRequired();

        builder.Property(p => p.SalesforceAccountId)
            .HasColumnName("salesforce_account_id");

        builder.Property(p => p.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(p => p.RefPartyId)
            .IsUnique()
            .HasDatabaseName("ix_parties_ref_party_id");

        builder.HasIndex(p => p.RefPublicId)
            .IsUnique()
            .HasDatabaseName("ix_parties_ref_public_id");

        builder.HasIndex(p => p.HomeCenterNumber)
            .HasDatabaseName("ix_parties_home_center_number");

        builder.HasOne(p => p.Person)
            .WithOne(pp => pp.Party)
            .HasForeignKey<PartyPersonCache>(pp => pp.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Organization)
            .WithOne(po => po.Party)
            .HasForeignKey<PartyOrganizationCache>(po => po.PartyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
