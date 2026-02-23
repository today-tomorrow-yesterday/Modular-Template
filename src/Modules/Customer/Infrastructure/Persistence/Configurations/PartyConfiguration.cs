using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> builder)
    {
        builder.ToTable("parties");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.PublicId)
            .HasColumnName("public_id")
            .IsRequired();

        builder.HasIndex(p => p.PublicId)
            .IsUnique()
            .HasDatabaseName("uq_parties_public_id");

        // TPH discriminator — "party_type" column stores "Person" or "Organization"
        builder.HasDiscriminator(p => p.PartyType)
            .HasValue<Person>(PartyType.Person)
            .HasValue<Organization>(PartyType.Organization);

        builder.Property(p => p.PartyType)
            .HasColumnName("party_type")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.LifecycleStage)
            .HasColumnName("lifecycle_stage")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.HomeCenterNumber)
            .HasColumnName("home_center_number")
            .IsRequired();

        builder.Property(p => p.SalesforceUrl)
            .HasColumnName("salesforce_url")
            .HasMaxLength(500);

        // MailingAddress — owned value object, flattened columns on parties table
        builder.OwnsOne(p => p.MailingAddress, addrBuilder =>
        {
            addrBuilder.Property(a => a.AddressLine1)
                .HasColumnName("mailing_address_line1")
                .HasMaxLength(500);

            addrBuilder.Property(a => a.AddressLine2)
                .HasColumnName("mailing_address_line2")
                .HasMaxLength(500);

            addrBuilder.Property(a => a.City)
                .HasColumnName("mailing_city")
                .HasMaxLength(200);

            addrBuilder.Property(a => a.County)
                .HasColumnName("mailing_county")
                .HasMaxLength(200);

            addrBuilder.Property(a => a.State)
                .HasColumnName("mailing_state")
                .HasMaxLength(50);

            addrBuilder.Property(a => a.Country)
                .HasColumnName("mailing_country")
                .HasMaxLength(100);

            addrBuilder.Property(a => a.PostalCode)
                .HasColumnName("mailing_postal_code")
                .HasMaxLength(20);
        });

        // Child collections (shared)
        builder.HasMany(p => p.ContactPoints)
            .WithOne()
            .HasForeignKey(cp => cp.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Identifiers)
            .WithOne()
            .HasForeignKey(pi => pi.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Source system timestamps
        builder.Property(p => p.SourceCreatedOn)
            .HasColumnName("source_created_on");

        builder.Property(p => p.SourceLastModifiedOn)
            .HasColumnName("source_last_modified_on");

        builder.Property(p => p.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.HomeCenterNumber)
            .HasDatabaseName("ix_parties_home_center_number");

        builder.HasIndex(p => p.LifecycleStage)
            .HasDatabaseName("ix_parties_lifecycle_stage");

        builder.Ignore(p => p.DomainEvents);
    }
}
