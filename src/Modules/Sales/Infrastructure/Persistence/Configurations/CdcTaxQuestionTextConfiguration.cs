using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CdcTaxQuestionTextConfiguration : IEntityTypeConfiguration<CdcTaxQuestionText>
{
    public void Configure(EntityTypeBuilder<CdcTaxQuestionText> builder)
    {
        builder.ToTable("tax_question_text", Schemas.Cdc);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.TaxQuestionId)
            .HasColumnName("tax_question_id")
            .IsRequired();

        builder.Property(e => e.QuestionNumber)
            .HasColumnName("question_number")
            .IsRequired();

        builder.Property(e => e.Text)
            .HasColumnName("text")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.InactivateDate)
            .HasColumnName("inactivate_date");

        builder.Property(e => e.InactivatedBy)
            .HasColumnName("inactivated_by");

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.ModifiedAtUtc)
            .HasColumnName("modified_at_utc");

        builder.HasIndex(e => e.QuestionNumber)
            .IsUnique()
            .HasDatabaseName("uq_cdc_tax_question_text_number");

        builder.HasOne(e => e.TaxQuestion)
            .WithMany(q => q.QuestionTexts)
            .HasForeignKey(e => e.TaxQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}
