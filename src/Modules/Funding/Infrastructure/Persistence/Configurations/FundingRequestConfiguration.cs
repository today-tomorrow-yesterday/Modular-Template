using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Funding.Domain.FundingRequests;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.Funding.Infrastructure.Persistence.Configurations;

internal sealed class FundingRequestConfiguration : IEntityTypeConfiguration<FundingRequest>
{
    public void Configure(EntityTypeBuilder<FundingRequest> builder)
    {
        builder.ToTable("funding_requests");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.SaleId)
            .HasColumnName("sale_id")
            .IsRequired();

        builder.Property(f => f.PackageId)
            .HasColumnName("package_id")
            .IsRequired();

        builder.Property(f => f.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(f => f.RefCustomerPublicId)
            .HasColumnName("ref_customer_public_id");

        builder.Property(f => f.RequestType)
            .HasColumnName("request_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.RequestAmount)
            .HasColumnName("request_amount")
            .IsRequired();

        builder.Property(f => f.ApprovalDate)
            .HasColumnName("approval_date");

        builder.Property(f => f.ApprovalExpirationDate)
            .HasColumnName("approval_expiration_date");

        builder.Property(f => f.LenderId)
            .HasColumnName("lender_id")
            .IsRequired();

        builder.Property(f => f.LenderName)
            .HasColumnName("lender_name")
            .HasMaxLength(200);

        builder.Property(f => f.HomeCenterNumber)
            .HasColumnName("home_center_number");

        builder.Property(f => f.FundingKeys)
            .HasColumnName("funding_keys")
            .HasColumnType("jsonb");

        builder.HasOne(f => f.Customer)
            .WithMany()
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditProperties();

        builder.HasIndex(f => f.SaleId)
            .HasDatabaseName("ix_funding_requests_sale_id");

        builder.HasIndex(f => f.CustomerId)
            .HasDatabaseName("ix_funding_requests_customer_id")
            .HasFilter("customer_id IS NOT NULL");

        builder.HasIndex(f => f.RefCustomerPublicId)
            .HasDatabaseName("ix_funding_requests_ref_customer_public_id")
            .HasFilter("ref_customer_public_id IS NOT NULL");
    }
}
