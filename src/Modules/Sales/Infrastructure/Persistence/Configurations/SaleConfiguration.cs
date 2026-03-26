using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(s => s.RetailLocationId)
            .HasColumnName("retail_location_id")
            .IsRequired();

        builder.Property(s => s.SaleType)
            .HasColumnName("sale_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.SaleStatus)
            .HasColumnName("sale_status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.SaleNumber)
            .HasColumnName("sale_number")
            .UseIdentityAlwaysColumn();

        builder.Property(s => s.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(s => s.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        builder.Property(s => s.DeletedByUserId)
            .HasColumnName("deleted_by_user_id");

        builder.ConfigureAuditProperties();

        builder.HasIndex(s => s.PublicId)
            .IsUnique()
            .HasDatabaseName("ix_sales_public_id");

        builder.HasIndex(s => s.CustomerId)
            .HasDatabaseName("ix_sales_customer_id");

        builder.HasIndex(s => s.RetailLocationId)
            .HasDatabaseName("ix_sales_retail_location_id");

        builder.HasIndex(s => s.SaleNumber)
            .HasDatabaseName("ix_sales_sale_number");

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.RetailLocation)
            .WithMany(r => r.Sales)
            .HasForeignKey(s => s.RetailLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
