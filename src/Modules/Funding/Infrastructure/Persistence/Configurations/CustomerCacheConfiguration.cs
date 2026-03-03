using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Funding.Domain.CustomersCache;

namespace Modules.Funding.Infrastructure.Persistence.Configurations;

internal sealed class CustomerCacheConfiguration : IEntityTypeConfiguration<CustomerCache>
{
    public void Configure(EntityTypeBuilder<CustomerCache> builder)
    {
        builder.ToTable("customers_cache", "cache");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(c => c.RefPublicId)
            .HasColumnName("ref_public_id")
            .IsRequired();

        builder.Property(c => c.LoanId)
            .HasColumnName("loan_id")
            .HasMaxLength(100);

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.HomeCenterNumber)
            .HasColumnName("home_center_number")
            .IsRequired();

        builder.Property(c => c.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(c => c.RefPublicId)
            .IsUnique()
            .HasDatabaseName("ix_customers_cache_ref_public_id");

        builder.HasIndex(c => c.LoanId)
            .HasDatabaseName("ix_customers_cache_loan_id");
    }
}
