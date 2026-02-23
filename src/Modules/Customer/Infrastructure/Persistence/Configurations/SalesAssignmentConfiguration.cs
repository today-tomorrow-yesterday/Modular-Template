using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Parties.Entities;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class SalesAssignmentConfiguration : IEntityTypeConfiguration<SalesAssignment>
{
    public void Configure(EntityTypeBuilder<SalesAssignment> builder)
    {
        builder.ToTable("sales_assignments");

        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(sa => sa.PersonId)
            .HasColumnName("person_id");

        builder.Property(sa => sa.SalesPersonId)
            .HasColumnName("sales_person_id")
            .HasMaxLength(200);

        builder.Property(sa => sa.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(50);

        // Exactly one Primary per Person; multiple Supporting allowed
        builder.HasIndex(sa => sa.PersonId)
            .IsUnique()
            .HasFilter("role = 'Primary'");

        // Prevent same SalesPerson assigned twice to the same Person
        builder.HasIndex(sa => new { sa.PersonId, sa.SalesPersonId })
            .IsUnique();

        builder.HasOne(sa => sa.SalesPerson)
            .WithMany()
            .HasForeignKey(sa => sa.SalesPersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
