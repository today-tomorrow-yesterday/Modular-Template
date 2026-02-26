using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Credits;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Packages.Land;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.Domain.Packages.Tax;
using Modules.Sales.Domain.Packages.TradeIns;
using Modules.Sales.Domain.Packages.Warranty;
using Rtl.Core.Infrastructure.Auditing.Configurations;
using Rtl.Core.Infrastructure.Persistence.Versioning;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class PackageLineConfiguration : IEntityTypeConfiguration<PackageLine>
{
    public void Configure(EntityTypeBuilder<PackageLine> builder)
    {
        builder.ToTable("package_lines", Schemas.Packages);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.PackageId)
            .HasColumnName("package_id")
            .IsRequired();

        builder.Property(l => l.LineType)
            .HasColumnName("line_type")
            .IsRequired();

        builder.Property(l => l.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(l => l.Responsibility)
            .HasColumnName("responsibility")
            .HasConversion<string>();

        builder.Property(l => l.EstimatedCost)
            .HasColumnName("estimated_cost");

        builder.Property(l => l.RetailSalePrice)
            .HasColumnName("retail_sale_price");

        builder.Property(l => l.SalePrice)
            .HasColumnName("sale_price");

        builder.Property(l => l.ShouldExcludeFromPricing)
            .HasColumnName("should_exclude_from_pricing");

        builder.ConfigureAuditProperties();

        builder.HasIndex(l => l.PackageId)
            .HasDatabaseName("ix_package_lines_package_id");

        // TPH discriminator
        builder.HasDiscriminator(l => l.LineType)
            .HasValue<HomeLine>(PackageLineTypeConstants.Home)
            .HasValue<LandLine>(PackageLineTypeConstants.Land)
            .HasValue<TaxLine>(PackageLineTypeConstants.Tax)
            .HasValue<InsuranceLine>(PackageLineTypeConstants.Insurance)
            .HasValue<WarrantyLine>(PackageLineTypeConstants.Warranty)
            .HasValue<TradeInLine>(PackageLineTypeConstants.TradeIn)
            .HasValue<SalesTeamLine>(PackageLineTypeConstants.SalesTeam)
            .HasValue<ProjectCostLine>(PackageLineTypeConstants.ProjectCost)
            .HasValue<CreditLine>(PackageLineTypeConstants.Credit);
    }
}

internal sealed class HomeLineConfiguration : IEntityTypeConfiguration<HomeLine>
{
    public void Configure(EntityTypeBuilder<HomeLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("home_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<HomeDetails>());

        // FK to inventory product cache (nullable — NULL for manual homes)
        builder.Property(l => l.OnLotHomeId)
            .HasColumnName("on_lot_home_id");

        builder.HasOne(l => l.OnLotHome)
            .WithMany()
            .HasForeignKey(l => l.OnLotHomeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(l => l.OnLotHomeId)
            .HasDatabaseName("ix_package_lines_on_lot_home_id")
            .HasFilter("on_lot_home_id IS NOT NULL");
    }
}

internal sealed class LandLineConfiguration : IEntityTypeConfiguration<LandLine>
{
    public void Configure(EntityTypeBuilder<LandLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("land_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<LandDetails>());

        // FK to inventory product cache (nullable — NULL for customer-owned/private/community land)
        builder.Property(l => l.LandParcelId)
            .HasColumnName("land_parcel_id");

        builder.HasOne(l => l.LandParcel)
            .WithMany()
            .HasForeignKey(l => l.LandParcelId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(l => l.LandParcelId)
            .HasDatabaseName("ix_package_lines_land_parcel_id")
            .HasFilter("land_parcel_id IS NOT NULL");
    }
}

internal sealed class InsuranceLineConfiguration : IEntityTypeConfiguration<InsuranceLine>
{
    public void Configure(EntityTypeBuilder<InsuranceLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("insurance_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<InsuranceDetails>());
    }
}

internal sealed class WarrantyLineConfiguration : IEntityTypeConfiguration<WarrantyLine>
{
    public void Configure(EntityTypeBuilder<WarrantyLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("warranty_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<WarrantyDetails>());
    }
}

internal sealed class CreditLineConfiguration : IEntityTypeConfiguration<CreditLine>
{
    public void Configure(EntityTypeBuilder<CreditLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("credit_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<CreditDetails>());
    }
}

internal sealed class ProjectCostLineConfiguration : IEntityTypeConfiguration<ProjectCostLine>
{
    public void Configure(EntityTypeBuilder<ProjectCostLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("project_cost_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<ProjectCostDetails>());
    }
}

internal sealed class TradeInLineConfiguration : IEntityTypeConfiguration<TradeInLine>
{
    public void Configure(EntityTypeBuilder<TradeInLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("trade_in_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<TradeInDetails>());
    }
}

internal sealed class SalesTeamLineConfiguration : IEntityTypeConfiguration<SalesTeamLine>
{
    public void Configure(EntityTypeBuilder<SalesTeamLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("sales_team_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<SalesTeamDetails>());
    }
}

internal sealed class TaxLineConfiguration : IEntityTypeConfiguration<TaxLine>
{
    public void Configure(EntityTypeBuilder<TaxLine> builder)
    {
        builder.Property(l => l.Details)
            .HasColumnName("tax_details")
            .HasColumnType("jsonb")
            .HasConversion(new VersionedJsonConverter<TaxDetails>());
    }
}
