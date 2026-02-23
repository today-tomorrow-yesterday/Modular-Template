using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcProjectCostItemConfiguration : IEntityTypeConfiguration<CdcProjectCostItem>
{
    public void Configure(EntityTypeBuilder<CdcProjectCostItem> builder)
    {
        builder.ToTable("project_cost_item", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.MasterDealer)
            .HasColumnName("master_dealer")
            .IsRequired();

        builder.Property(e => e.ProjectCostCategoryId)
            .HasColumnName("project_cost_category_id")
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(e => e.ItemNumber)
            .HasColumnName("item_number")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(e => e.IsFeeItem)
            .HasColumnName("is_fee_item")
            .IsRequired();

        builder.Property(e => e.IsCssRestricted)
            .HasColumnName("is_css_restricted")
            .IsRequired();

        builder.Property(e => e.IsFhaRestricted)
            .HasColumnName("is_fha_restricted")
            .IsRequired();

        builder.Property(e => e.IsDisplayForCash)
            .HasColumnName("is_display_for_cash")
            .IsRequired();

        builder.Property(e => e.IsRestrictOptionPrice)
            .HasColumnName("is_restrict_option_price")
            .IsRequired();

        builder.Property(e => e.IsRestrictCssCost)
            .HasColumnName("is_restrict_css_cost")
            .IsRequired();

        builder.Property(e => e.IsHopeRefundsIncluded)
            .HasColumnName("is_hope_refunds_included")
            .IsRequired();

        builder.Property(e => e.ProfitPercentage)
            .HasColumnName("profit_percentage")
            .HasPrecision(7, 2);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => new { e.MasterDealer, e.CategoryId, e.ItemNumber })
            .IsUnique()
            .HasDatabaseName("uq_cdc_project_cost_item_dealer_cat_number");

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(e => e.ProjectCostCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}
