using FluentValidation;

namespace Modules.Sales.Application.DeliveryAddresses.UpdateDeliveryAddress;

internal sealed class UpdateDeliveryAddressCommandValidator
    : AbstractValidator<UpdateDeliveryAddressCommand>
{
    private static readonly string[] ValidOccupancyTypes =
    [
        "Primary Residence",
        "Secondary Residence",
        "Buy for Immediate Family",
        "Buy for Other",
        "Rental",
        "Investment"
    ];

    private static readonly HashSet<string> ValidStateAbbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
        "DC", "PR", "VI", "GU", "AS", "MP"
    };

    public UpdateDeliveryAddressCommandValidator()
    {
        RuleFor(x => x.SalePublicId)
            .NotEmpty();

        RuleFor(x => x.State)
            .MaximumLength(2)
            .Must(state => ValidStateAbbreviations.Contains(state!))
            .When(x => x.State is not null)
            .WithMessage("State must be a valid 2-letter US state abbreviation.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(10)
            .When(x => x.PostalCode is not null);

        RuleFor(x => x.OccupancyType)
            .Must(type => ValidOccupancyTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .When(x => x.OccupancyType is not null)
            .WithMessage("OccupancyType must be one of: " + string.Join(", ", ValidOccupancyTypes));
    }
}
