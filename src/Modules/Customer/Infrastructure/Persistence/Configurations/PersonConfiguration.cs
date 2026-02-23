using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Parties.Entities;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        // PersonName — owned value object, flattened columns on parties table
        builder.OwnsOne(p => p.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(200);

            nameBuilder.Property(n => n.MiddleName)
                .HasColumnName("middle_name")
                .HasMaxLength(200);

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(200);

            nameBuilder.Property(n => n.NameExtension)
                .HasColumnName("name_extension")
                .HasMaxLength(20);
        });

        builder.Property(p => p.DateOfBirth)
            .HasColumnName("date_of_birth");

        // SalesAssignments — collection via backing field, cascade delete with Person
        builder.HasMany(p => p.SalesAssignments)
            .WithOne(sa => sa.Person)
            .HasForeignKey(sa => sa.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.SalesAssignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // CoBuyer — self-referencing FK to another Party
        builder.Property(p => p.CoBuyerPartyId)
            .HasColumnName("co_buyer_party_id");

        builder.HasOne(p => p.CoBuyer)
            .WithMany()
            .HasForeignKey(p => p.CoBuyerPartyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
