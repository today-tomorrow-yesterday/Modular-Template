using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.PartiesCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class PartyOrganizationCacheConfiguration : IEntityTypeConfiguration<PartyOrganizationCache>
{
    public void Configure(EntityTypeBuilder<PartyOrganizationCache> builder)
    {
        builder.ToTable("party_organizations", Schemas.Cache);

        builder.HasKey(po => po.PartyId);

        builder.Property(po => po.PartyId)
            .HasColumnName("party_id")
            .ValueGeneratedNever();

        builder.Property(po => po.OrganizationName)
            .HasColumnName("organization_name")
            .IsRequired();
    }
}
