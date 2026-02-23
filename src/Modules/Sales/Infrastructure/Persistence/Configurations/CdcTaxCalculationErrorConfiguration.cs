using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcTaxCalculationErrorConfiguration : IEntityTypeConfiguration<CdcTaxCalculationError>
{
    public void Configure(EntityTypeBuilder<CdcTaxCalculationError> builder)
    {
        builder.ToTable("tax_calculation_error", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.FundingId)
            .HasColumnName("funding_id")
            .IsRequired();

        builder.Property(e => e.LinkId)
            .HasColumnName("link_id");

        builder.Property(e => e.SequenceNumber)
            .HasColumnName("sequence_number")
            .IsRequired();

        builder.Property(e => e.Message)
            .HasColumnName("message")
            .IsRequired();

        builder.Property(e => e.MasterDealer)
            .HasColumnName("master_dealer")
            .IsRequired();

        builder.Property(e => e.HomeCenterNumber)
            .HasColumnName("home_center_number")
            .IsRequired();

        builder.Property(e => e.FieldName)
            .HasColumnName("field_name")
            .IsRequired();

        builder.Property(e => e.MessageId)
            .HasColumnName("message_id")
            .IsRequired();

        builder.Property(e => e.ProgramName)
            .HasColumnName("program_name")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => new { e.FundingId, e.SequenceNumber })
            .IsUnique()
            .HasDatabaseName("uq_cdc_tax_calc_error_funding_seq");

        builder.HasOne(e => e.FundingRequest)
            .WithMany()
            .HasForeignKey(e => e.FundingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
