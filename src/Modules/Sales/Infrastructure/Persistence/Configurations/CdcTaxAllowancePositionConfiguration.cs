using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcTaxAllowancePositionConfiguration : IEntityTypeConfiguration<CdcTaxAllowancePosition>
{
    public void Configure(EntityTypeBuilder<CdcTaxAllowancePosition> builder)
    {
        builder.ToTable("tax_allowance_position", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(e => e.TypeCode)
            .HasColumnName("type_code")
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(e => e.CostGlClayton)
            .HasColumnName("cost_gl_clayton")
            .IsRequired();

        builder.Property(e => e.SaleGlClayton)
            .HasColumnName("sale_gl_clayton")
            .IsRequired();

        builder.Property(e => e.CostGlGlobal)
            .HasColumnName("cost_gl_global")
            .IsRequired();

        builder.Property(e => e.SaleGlGlobal)
            .HasColumnName("sale_gl_global")
            .IsRequired();

        builder.Property(e => e.IsMandatory)
            .HasColumnName("is_mandatory")
            .IsRequired();

        builder.Property(e => e.IsMandatorySale)
            .HasColumnName("is_mandatory_sale")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => e.Position)
            .IsUnique()
            .HasDatabaseName("uq_cdc_tax_allowance_position");

        builder.Ignore(e => e.DomainEvents);
    }
}
