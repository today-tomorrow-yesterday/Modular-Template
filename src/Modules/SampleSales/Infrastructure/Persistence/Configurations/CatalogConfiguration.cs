using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.SampleSales.Domain.Catalogs;
using Rtl.Core.Infrastructure.Auditing.Configurations;

namespace Modules.SampleSales.Infrastructure.Persistence.Configurations;

internal sealed class CatalogConfiguration : IEntityTypeConfiguration<Catalog>
{
    public void Configure(EntityTypeBuilder<Catalog> builder)
    {
        builder.ToTable("catalogs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        // Configure Products collection
        builder.HasMany(c => c.Products)
            .WithOne()
            .HasForeignKey(cp => cp.CatalogId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure audit fields from IAuditableEntity
        builder.ConfigureAuditProperties();

        // Configure soft delete fields from ISoftDeletable
        builder.ConfigureSoftDeleteProperties();
    }
}
