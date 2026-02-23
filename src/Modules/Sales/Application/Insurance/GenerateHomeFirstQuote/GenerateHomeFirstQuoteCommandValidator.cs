using FluentValidation;

namespace Modules.Sales.Application.Insurance.GenerateHomeFirstQuote;

internal sealed class GenerateHomeFirstQuoteCommandValidator
    : AbstractValidator<GenerateHomeFirstQuoteCommand>
{
    private static readonly char[] ValidOccupancyTypes = ['P', 'p', 'S', 's', 'R', 'r'];

    private static readonly HashSet<string> ValidStateAbbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
        "DC", "PR", "VI", "GU", "AS", "MP"
    };

    public GenerateHomeFirstQuoteCommandValidator()
    {
        RuleFor(x => x.SalePublicId)
            .NotEmpty();

        RuleFor(x => x.CoverageAmount)
            .GreaterThan(0)
            .WithMessage("CoverageAmount must be greater than zero.");

        RuleFor(x => x.OccupancyType)
            .Must(o => ValidOccupancyTypes.Contains(o))
            .WithMessage("OccupancyType must be 'P' (Primary), 'S' (Secondary), or 'R' (Rental).");

        RuleFor(x => x.CustomerBirthDate)
            .NotEmpty()
            .LessThan(DateTime.UtcNow)
            .WithMessage("CustomerBirthDate must be a valid date in the past.");

        RuleFor(x => x.CoApplicantBirthDate)
            .LessThan(DateTime.UtcNow)
            .When(x => x.CoApplicantBirthDate.HasValue)
            .WithMessage("CoApplicantBirthDate must be a valid date in the past.");

        RuleFor(x => x.MailingAddress)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.MailingCity)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.MailingState)
            .NotEmpty()
            .MaximumLength(2)
            .Must(state => ValidStateAbbreviations.Contains(state))
            .WithMessage("MailingState must be a valid 2-letter US state abbreviation.");

        RuleFor(x => x.MailingZip)
            .NotEmpty()
            .MaximumLength(10);
    }
}
