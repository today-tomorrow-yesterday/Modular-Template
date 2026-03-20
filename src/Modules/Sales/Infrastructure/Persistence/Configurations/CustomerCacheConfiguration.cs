using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.CustomersCache;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CustomerCacheConfiguration : IEntityTypeConfiguration<CustomerCache>
{
    public void Configure(EntityTypeBuilder<CustomerCache> builder)
    {
        builder.ToTable("customers", Schemas.Cache);
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(c => c.RefPublicId).HasColumnName("ref_public_id").IsRequired();
        builder.HasIndex(c => c.RefPublicId).IsUnique().HasDatabaseName("uq_customers_ref_public_id");
        builder.Property(c => c.LifecycleStage).HasColumnName("lifecycle_stage").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(c => c.HomeCenterNumber).HasColumnName("home_center_number").IsRequired();
        builder.Property(c => c.DisplayName).HasColumnName("display_name").HasMaxLength(500).IsRequired();
        builder.Property(c => c.SalesforceAccountId).HasColumnName("salesforce_account_id").HasMaxLength(200);
        builder.Property(c => c.LastSyncedAtUtc).HasColumnName("last_synced_at_utc").IsRequired();
        builder.Property(c => c.FirstName).HasColumnName("first_name").HasMaxLength(200);
        builder.Property(c => c.MiddleName).HasColumnName("middle_name").HasMaxLength(200);
        builder.Property(c => c.LastName).HasColumnName("last_name").HasMaxLength(200);
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(500);
        builder.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(200);
        builder.Property(c => c.CoBuyerFirstName).HasColumnName("co_buyer_first_name").HasMaxLength(200);
        builder.Property(c => c.CoBuyerLastName).HasColumnName("co_buyer_last_name").HasMaxLength(200);
        builder.Property(c => c.PrimarySalesPersonFederatedId).HasColumnName("primary_sales_person_federated_id").HasMaxLength(200);
        builder.Property(c => c.PrimarySalesPersonFirstName).HasColumnName("primary_sales_person_first_name").HasMaxLength(200);
        builder.Property(c => c.PrimarySalesPersonLastName).HasColumnName("primary_sales_person_last_name").HasMaxLength(200);
        builder.Property(c => c.SecondarySalesPersonFederatedId).HasColumnName("secondary_sales_person_federated_id").HasMaxLength(200);
        builder.Property(c => c.SecondarySalesPersonFirstName).HasColumnName("secondary_sales_person_first_name").HasMaxLength(200);
        builder.Property(c => c.SecondarySalesPersonLastName).HasColumnName("secondary_sales_person_last_name").HasMaxLength(200);
        builder.HasIndex(c => c.PrimarySalesPersonFederatedId).HasDatabaseName("ix_customers_primary_sp_federated_id");
    }
}
