using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Parties.Entities;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(o => o.OrganizationName)
            .HasColumnName("organization_name")
            .HasMaxLength(500);
    }
}
