using FluentValidation;

namespace Modules.Sales.Application.Packages.UpdatePackageTradeIns;

internal sealed class UpdatePackageTradeInsCommandValidator
    : AbstractValidator<UpdatePackageTradeInsCommand>
{
    private static readonly string[] ValidTradeTypes =
        ["Manufactured Home", "Modular Home", "Auto", "Motorcycle", "RV", "Watercraft"];

    private static readonly string[] HomeTradeTypes =
        ["Manufactured Home", "Modular Home"];

    public UpdatePackageTradeInsCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotNull()
            .Must(items => items.Length <= 5)
            .WithMessage("A package cannot have more than 5 trade-in items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.SalePrice)
                .GreaterThanOrEqualTo(0);

            item.RuleFor(i => i.EstimatedCost)
                .GreaterThanOrEqualTo(0);

            item.RuleFor(i => i.RetailSalePrice)
                .GreaterThanOrEqualTo(0);

            item.RuleFor(i => i.TradeType)
                .NotEmpty()
                .Must(t => ValidTradeTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"TradeType must be one of: {string.Join(", ", ValidTradeTypes)}.");

            item.RuleFor(i => i.Year)
                .GreaterThan(1900);

            item.RuleFor(i => i.Make)
                .NotEmpty();

            item.RuleFor(i => i.Model)
                .NotEmpty();

            // FloorWidth/FloorLength required for home trade types (Manufactured Home, Modular Home)
            item.RuleFor(i => i.FloorWidth)
                .NotNull()
                .GreaterThanOrEqualTo(0)
                .When(i => HomeTradeTypes.Contains(i.TradeType, StringComparer.OrdinalIgnoreCase))
                .WithMessage("FloorWidth is required for Manufactured Home and Modular Home trade types.");

            item.RuleFor(i => i.FloorWidth)
                .GreaterThanOrEqualTo(0)
                .When(i => i.FloorWidth.HasValue && !HomeTradeTypes.Contains(i.TradeType, StringComparer.OrdinalIgnoreCase));

            item.RuleFor(i => i.FloorLength)
                .NotNull()
                .GreaterThanOrEqualTo(0)
                .When(i => HomeTradeTypes.Contains(i.TradeType, StringComparer.OrdinalIgnoreCase))
                .WithMessage("FloorLength is required for Manufactured Home and Modular Home trade types.");

            item.RuleFor(i => i.FloorLength)
                .GreaterThanOrEqualTo(0)
                .When(i => i.FloorLength.HasValue && !HomeTradeTypes.Contains(i.TradeType, StringComparer.OrdinalIgnoreCase));

            item.RuleFor(i => i.TradeAllowance)
                .GreaterThanOrEqualTo(0);

            item.RuleFor(i => i.PayoffAmount)
                .GreaterThanOrEqualTo(0);

            item.RuleFor(i => i.BookInAmount)
                .GreaterThanOrEqualTo(0);
        });
    }
}
