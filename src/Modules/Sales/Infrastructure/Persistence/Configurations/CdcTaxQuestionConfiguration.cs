using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcTaxQuestionConfiguration : IEntityTypeConfiguration<CdcTaxQuestion>
{
    public void Configure(EntityTypeBuilder<CdcTaxQuestion> builder)
    {
        builder.ToTable("tax_question", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.MasterDealer)
            .HasColumnName("master_dealer")
            .IsRequired();

        builder.Property(e => e.StateCode)
            .HasColumnName("state_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(e => e.QuestionNumber)
            .HasColumnName("question_number")
            .IsRequired();

        builder.Property(e => e.EffectiveDate)
            .HasColumnName("effective_date")
            .IsRequired();

        builder.Property(e => e.EndDate)
            .HasColumnName("end_date");

        builder.Property(e => e.AskForNew)
            .HasColumnName("ask_for_new")
            .IsRequired();

        builder.Property(e => e.AskForUsed)
            .HasColumnName("ask_for_used")
            .IsRequired();

        builder.Property(e => e.AskForRepo)
            .HasColumnName("ask_for_repo")
            .IsRequired();

        builder.Property(e => e.AskForLand)
            .HasColumnName("ask_for_land")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => new { e.MasterDealer, e.StateCode, e.QuestionNumber })
            .IsUnique()
            .HasDatabaseName("uq_cdc_tax_question_dealer_state_number");

        builder.Ignore(e => e.DomainEvents);
    }
}
