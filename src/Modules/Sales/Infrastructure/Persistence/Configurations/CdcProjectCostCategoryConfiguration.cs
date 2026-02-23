using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcProjectCostCategoryConfiguration : IEntityTypeConfiguration<CdcProjectCostCategory>
{
    public void Configure(EntityTypeBuilder<CdcProjectCostCategory> builder)
    {
        builder.ToTable("project_cost_category", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.MasterDealer)
            .HasColumnName("master_dealer")
            .IsRequired();

        builder.Property(e => e.CategoryNumber)
            .HasColumnName("category_number")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(e => e.IsCreditConsideration)
            .HasColumnName("is_credit_consideration")
            .IsRequired();

        builder.Property(e => e.IsLandDot)
            .HasColumnName("is_land_dot")
            .IsRequired();

        builder.Property(e => e.RestrictFha)
            .HasColumnName("restrict_fha")
            .IsRequired();

        builder.Property(e => e.RestrictCss)
            .HasColumnName("restrict_css")
            .IsRequired();

        builder.Property(e => e.DisplayForCash)
            .HasColumnName("display_for_cash")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => new { e.MasterDealer, e.CategoryNumber })
            .IsUnique()
            .HasDatabaseName("uq_cdc_project_cost_category_dealer_number");

        builder.Ignore(e => e.DomainEvents);
    }
}
