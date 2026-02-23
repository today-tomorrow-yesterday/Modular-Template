using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.FundingCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class FundingRequestCacheConfiguration : IEntityTypeConfiguration<FundingRequestCache>
{
    public void Configure(EntityTypeBuilder<FundingRequestCache> builder)
    {
        builder.ToTable("funding_requests_cache", Schemas.Cache);

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(f => f.RefFundingRequestId)
            .HasColumnName("ref_funding_request_id")
            .IsRequired();

        builder.Property(f => f.SaleId)
            .HasColumnName("sale_id")
            .IsRequired();

        builder.Property(f => f.PackageId)
            .HasColumnName("package_id")
            .IsRequired();

        builder.Property(f => f.LenderId)
            .HasColumnName("lender_id")
            .IsRequired();

        builder.Property(f => f.LenderName)
            .HasColumnName("lender_name");

        builder.Property(f => f.RequestAmount)
            .HasColumnName("request_amount")
            .IsRequired();

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(f => f.ApprovalDate)
            .HasColumnName("approval_date");

        builder.Property(f => f.ApprovalExpirationDate)
            .HasColumnName("approval_expiration_date");

        builder.Property(f => f.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        builder.HasIndex(f => f.RefFundingRequestId)
            .IsUnique()
            .HasDatabaseName("ix_funding_requests_cache_ref_funding_request_id");

        builder.HasIndex(f => f.SaleId)
            .HasDatabaseName("ix_funding_requests_cache_sale_id");

        builder.HasOne(f => f.Sale)
            .WithMany(s => s.FundingRequests)
            .HasForeignKey(f => f.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Package)
            .WithMany()
            .HasForeignKey(f => f.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
