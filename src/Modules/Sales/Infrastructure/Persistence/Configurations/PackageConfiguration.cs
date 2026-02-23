using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Packages;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("packages", Schemas.Packages);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.PublicId)
            .HasColumnName("public_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.SaleId)
            .HasColumnName("sale_id")
            .IsRequired();

        builder.Property(p => p.Version)
            .HasColumnName("version");

        builder.Property(p => p.Ranking)
            .HasColumnName("ranking")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(p => p.GrossProfit)
            .HasColumnName("gross_profit");

        builder.Property(p => p.CommissionableGrossProfit)
            .HasColumnName("commissionable_gross_profit");

        builder.Property(p => p.MustRecalculateTaxes)
            .HasColumnName("must_recalculate_taxes");

        builder.HasOne(p => p.Sale)
            .WithMany(s => s.Packages)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Lines)
            .WithOne(l => l.Package)
            .HasForeignKey(l => l.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.IsPrimaryPackage);

        builder.ConfigureAuditProperties();

        builder.HasIndex(p => p.PublicId)
            .IsUnique()
            .HasDatabaseName("ix_packages_public_id");

        builder.HasIndex(p => p.SaleId)
            .HasDatabaseName("ix_packages_sale_id");
    }
}
