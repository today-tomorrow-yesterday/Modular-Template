using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.PartiesCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class PartyPersonCacheConfiguration : IEntityTypeConfiguration<PartyPersonCache>
{
    public void Configure(EntityTypeBuilder<PartyPersonCache> builder)
    {
        builder.ToTable("party_persons", Schemas.Cache);

        builder.HasKey(pp => pp.PartyId);

        builder.Property(pp => pp.PartyId)
            .HasColumnName("party_id")
            .ValueGeneratedNever();

        builder.Property(pp => pp.FirstName)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(pp => pp.MiddleName)
            .HasColumnName("middle_name");

        builder.Property(pp => pp.LastName)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(pp => pp.Email)
            .HasColumnName("email");

        builder.Property(pp => pp.Phone)
            .HasColumnName("phone");

        builder.Property(pp => pp.CoBuyerFirstName)
            .HasColumnName("co_buyer_first_name");

        builder.Property(pp => pp.CoBuyerLastName)
            .HasColumnName("co_buyer_last_name");

        builder.Property(pp => pp.PrimarySalesPersonFederatedId)
            .HasColumnName("primary_sales_person_federated_id");

        builder.Property(pp => pp.PrimarySalesPersonFirstName)
            .HasColumnName("primary_sales_person_first_name");

        builder.Property(pp => pp.PrimarySalesPersonLastName)
            .HasColumnName("primary_sales_person_last_name");

        builder.Property(pp => pp.SecondarySalesPersonFederatedId)
            .HasColumnName("secondary_sales_person_federated_id");

        builder.Property(pp => pp.SecondarySalesPersonFirstName)
            .HasColumnName("secondary_sales_person_first_name");

        builder.Property(pp => pp.SecondarySalesPersonLastName)
            .HasColumnName("secondary_sales_person_last_name");

        builder.HasIndex(pp => pp.PrimarySalesPersonFederatedId)
            .HasDatabaseName("ix_party_persons_primary_sp_federated_id");
    }
}
