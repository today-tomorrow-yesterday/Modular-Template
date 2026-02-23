using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.SalesPersons;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class SalesPersonConfiguration : IEntityTypeConfiguration<SalesPerson>
{
    public void Configure(EntityTypeBuilder<SalesPerson> builder)
    {
        builder.ToTable("salespersons");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Id)
            .HasColumnName("id")
            .HasMaxLength(200)
            .ValueGeneratedNever();

        builder.Property(sp => sp.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(sp => sp.Username)
            .HasColumnName("username")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sp => sp.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sp => sp.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sp => sp.LotNumber)
            .HasColumnName("lot_number");

        builder.Property(sp => sp.FederatedId)
            .HasColumnName("federated_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sp => sp.LastSyncedAtUtc)
            .HasColumnName("last_synced_at_utc")
            .IsRequired();

        // Unique index on FederatedId - critical cross-module correlation key
        builder.HasIndex(sp => sp.FederatedId)
            .IsUnique()
            .HasDatabaseName("uq_salespersons_federated_id");
    }
}
