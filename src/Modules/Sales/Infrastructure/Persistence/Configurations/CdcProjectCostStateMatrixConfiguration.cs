using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcProjectCostStateMatrixConfiguration : IEntityTypeConfiguration<CdcProjectCostStateMatrix>
{
    public void Configure(EntityTypeBuilder<CdcProjectCostStateMatrix> builder)
    {
        builder.ToTable("project_cost_state_matrix", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.MasterDealer)
            .HasColumnName("master_dealer")
            .IsRequired();

        builder.Property(e => e.ProjectCostCategoryId)
            .HasColumnName("project_cost_category_id")
            .IsRequired();

        builder.Property(e => e.ProjectCostItemId)
            .HasColumnName("project_cost_item_id")
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(e => e.CategoryItemId)
            .HasColumnName("category_item_id")
            .IsRequired();

        builder.Property(e => e.HomeType)
            .HasColumnName("home_type")
            .IsRequired();

        builder.Property(e => e.StateCode)
            .HasColumnName("state_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(e => e.TaxBasisManufactured)
            .HasColumnName("tax_basis_manufactured")
            .HasPrecision(4, 3)
            .IsRequired();

        builder.Property(e => e.TaxBasisModularOn)
            .HasColumnName("tax_basis_modular_on")
            .HasPrecision(4, 3)
            .IsRequired();

        builder.Property(e => e.TaxBasisModularOff)
            .HasColumnName("tax_basis_modular_off")
            .HasPrecision(4, 3)
            .IsRequired();

        builder.Property(e => e.IsInsurable)
            .HasColumnName("is_insurable")
            .IsRequired();

        builder.Property(e => e.IsAdjStructInsurable)
            .HasColumnName("is_adj_struct_insurable");

        builder.Property(e => e.IsTotalImprovementIncluded)
            .HasColumnName("is_total_improvement_included");

        builder.Property(e => e.IsFeeItemAllowed)
            .HasColumnName("is_fee_item_allowed");

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => new { e.MasterDealer, e.CategoryId, e.CategoryItemId, e.HomeType, e.StateCode })
            .IsUnique()
            .HasDatabaseName("uq_cdc_project_cost_state_matrix_composite");

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.ProjectCostCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Item)
            .WithMany()
            .HasForeignKey(e => e.ProjectCostItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
