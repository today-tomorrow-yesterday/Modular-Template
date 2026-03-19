using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customer.Domain.Customers.Entities;

namespace Modules.Customer.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Domain.Customers.Entities.Customer>
{
    public void Configure(EntityTypeBuilder<Domain.Customers.Entities.Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.PublicId)
            .HasColumnName("public_id")
            .IsRequired();

        builder.HasIndex(c => c.PublicId)
            .IsUnique()
            .HasDatabaseName("uq_customers_public_id");

        builder.Property(c => c.LifecycleStage)
            .HasColumnName("lifecycle_stage")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.HomeCenterNumber)
            .HasColumnName("home_center_number")
            .IsRequired();

        builder.Property(c => c.SalesforceUrl)
            .HasColumnName("salesforce_url")
            .HasMaxLength(500);

        // MailingAddress — owned value object, flattened columns
        builder.OwnsOne(c => c.MailingAddress, addrBuilder =>
        {
            addrBuilder.Property(a => a.AddressLine1).HasColumnName("mailing_address_line1").HasMaxLength(500);
            addrBuilder.Property(a => a.AddressLine2).HasColumnName("mailing_address_line2").HasMaxLength(500);
            addrBuilder.Property(a => a.City).HasColumnName("mailing_city").HasMaxLength(200);
            addrBuilder.Property(a => a.County).HasColumnName("mailing_county").HasMaxLength(200);
            addrBuilder.Property(a => a.State).HasColumnName("mailing_state").HasMaxLength(50);
            addrBuilder.Property(a => a.Country).HasColumnName("mailing_country").HasMaxLength(100);
            addrBuilder.Property(a => a.PostalCode).HasColumnName("mailing_postal_code").HasMaxLength(20);
        });

        // CustomerName — owned value object, flattened columns
        builder.OwnsOne(c => c.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName).HasColumnName("first_name").HasMaxLength(200);
            nameBuilder.Property(n => n.MiddleName).HasColumnName("middle_name").HasMaxLength(200);
            nameBuilder.Property(n => n.LastName).HasColumnName("last_name").HasMaxLength(200);
            nameBuilder.Property(n => n.NameExtension).HasColumnName("name_extension").HasMaxLength(20);
        });

        builder.Property(c => c.DateOfBirth)
            .HasColumnName("date_of_birth");

        // Child collections
        builder.HasMany(c => c.ContactPoints)
            .WithOne()
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Identifiers)
            .WithOne()
            .HasForeignKey(ci => ci.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // SalesAssignments
        builder.HasMany(c => c.SalesAssignments)
            .WithOne(sa => sa.Customer)
            .HasForeignKey(sa => sa.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.SalesAssignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // CoBuyer — self-referencing FK
        builder.Property(c => c.CoBuyerCustomerId)
            .HasColumnName("co_buyer_customer_id");

        builder.HasOne(c => c.CoBuyer)
            .WithMany()
            .HasForeignKey(c => c.CoBuyerCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Source system timestamps
        builder.Property(c => c.SourceCreatedOn).HasColumnName("source_created_on");
        builder.Property(c => c.SourceLastModifiedOn).HasColumnName("source_last_modified_on");
        builder.Property(c => c.LastSyncedAtUtc).HasColumnName("last_synced_at_utc").IsRequired();

        // Indexes
        builder.HasIndex(c => c.HomeCenterNumber).HasDatabaseName("ix_customers_home_center_number");
        builder.HasIndex(c => c.LifecycleStage).HasDatabaseName("ix_customers_lifecycle_stage");

        builder.Ignore(c => c.DomainEvents);
    }
}
