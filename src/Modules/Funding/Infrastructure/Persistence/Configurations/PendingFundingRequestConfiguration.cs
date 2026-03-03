using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Funding.Domain.FundingRequests;

namespace Modules.Funding.Infrastructure.Persistence.Configurations;

internal sealed class PendingFundingRequestConfiguration : IEntityTypeConfiguration<PendingFundingRequest>
{
    public void Configure(EntityTypeBuilder<PendingFundingRequest> builder)
    {
        builder.ToTable("pending_funding_requests");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.LoanId)
            .HasColumnName("loan_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.SaleId)
            .HasColumnName("sale_id")
            .IsRequired();

        builder.Property(p => p.PackageId)
            .HasColumnName("package_id")
            .IsRequired();

        builder.Property(p => p.RefCustomerPublicId)
            .HasColumnName("ref_customer_public_id");

        builder.Property(p => p.RequestAmount)
            .HasColumnName("request_amount")
            .IsRequired();

        builder.Property(p => p.HomeCenterNumber)
            .HasColumnName("home_center_number");

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(p => p.FundingKeys)
            .HasColumnName("funding_keys")
            .HasColumnType("jsonb");

        builder.HasIndex(p => p.LoanId)
            .HasDatabaseName("ix_pending_funding_requests_loan_id");
    }
}
