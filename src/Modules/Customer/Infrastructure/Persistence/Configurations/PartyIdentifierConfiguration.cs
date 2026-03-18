using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Parties.Entities;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class PartyIdentifierConfiguration : IEntityTypeConfiguration<PartyIdentifier>
{
    public void Configure(EntityTypeBuilder<PartyIdentifier> builder)
    {
        builder.ToTable("party_identifiers");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Id)
            .HasColumnName("id");

        builder.Property(pi => pi.PartyId)
            .HasColumnName("party_id")
            .IsRequired();

        builder.Property(pi => pi.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pi => pi.Value)
            .HasColumnName("value")
            .HasMaxLength(200)
            .IsRequired();

        // One identifier per type per party
        builder.HasIndex(pi => new { pi.PartyId, pi.Type })
            .IsUnique()
            .HasDatabaseName("uq_party_identifiers_party_id_type");

        // Lookup by identifier value (e.g., find party by SalesforceAccountId)
        builder.HasIndex(pi => new { pi.Type, pi.Value })
            .HasDatabaseName("ix_party_identifiers_type_value");

        builder.Ignore(pi => pi.DomainEvents);
    }
}
