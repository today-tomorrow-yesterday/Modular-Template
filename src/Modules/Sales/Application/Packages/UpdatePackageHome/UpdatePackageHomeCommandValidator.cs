using FluentValidation;
using Modules.Sales.Domain.Packages.Home;

namespace Modules.Sales.Application.Packages.UpdatePackageHome;

internal sealed class UpdatePackageHomeCommandValidator
    : AbstractValidator<UpdatePackageHomeCommand>
{
    public UpdatePackageHomeCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.Home.HomeType)
            .IsInEnum();

        RuleFor(x => x.Home.HomeSourceType)
            .IsInEnum();

        RuleFor(x => x.Home.SalePrice)
            .GreaterThanOrEqualTo(0);

        // Used/Repo homes require SalePrice > 0 (legacy validation)
        RuleFor(x => x.Home.SalePrice)
            .GreaterThan(0)
            .When(x => x.Home.HomeType is HomeType.Used or HomeType.Repo)
            .WithMessage("SalePrice must be greater than zero for Used or Repo homes.");

        RuleFor(x => x.Home.EstimatedCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Home.RetailSalePrice)
            .GreaterThanOrEqualTo(x => x.Home.SalePrice)
            .WithMessage("RetailSalePrice must be greater than or equal to SalePrice.");

        RuleFor(x => x.Home.StockNumber)
            .NotEmpty()
            .When(x => x.Home.HomeSourceType is HomeSourceType.OnLot or HomeSourceType.VmfHomes)
            .WithMessage("StockNumber is required when HomeSourceType is OnLot or VmfHomes.");

        RuleFor(x => x.Home.ModularType)
            .IsInEnum()
            .When(x => x.Home.ModularType is not null);

        RuleFor(x => x.Home.WheelAndAxlesOption)
            .IsInEnum()
            .When(x => x.Home.WheelAndAxlesOption is not null);

        // Cost field validations (legacy validation rules)
        RuleFor(x => x.Home.BaseCost)
            .GreaterThan(0)
            .When(x => x.Home.BaseCost.HasValue)
            .WithMessage("BaseCost must be greater than zero when provided.");

        RuleFor(x => x.Home.OptionsCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Home.OptionsCost.HasValue);

        RuleFor(x => x.Home.InvoiceCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Home.InvoiceCost.HasValue);

        RuleFor(x => x.Home.NetInvoice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Home.NetInvoice.HasValue);

        RuleFor(x => x.Home.PartnerAssistance)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Home.PartnerAssistance.HasValue);
    }
}
