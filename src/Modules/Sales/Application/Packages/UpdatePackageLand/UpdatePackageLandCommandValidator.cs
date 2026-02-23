using FluentValidation;

namespace Modules.Sales.Application.Packages.UpdatePackageLand;

internal sealed class UpdatePackageLandCommandValidator
    : AbstractValidator<UpdatePackageLandCommand>
{
    private static readonly string[] ValidLandPurchaseTypes =
        ["CustomerHasLand", "CustomerWantsToPurchaseLand"];

    private static readonly string[] ValidCustomerLandTypes =
        ["CustomerOwnedLand", "PrivateProperty", "CommunityOrNeighborhood"];

    private static readonly string[] ValidLandInclusions =
        ["CustomerLandPayoff", "CustomerLandInLieu", "HomeOnly"];

    private static readonly string[] ValidTypesOfLandWanted =
        ["LandPurchase", "HomeCenterOwnedLand"];

    public UpdatePackageLandCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RetailSalePrice)
            .GreaterThanOrEqualTo(x => x.SalePrice)
            .WithMessage("RetailSalePrice must be greater than or equal to SalePrice.");

        RuleFor(x => x.LandPurchaseType)
            .NotEmpty()
            .Must(t => ValidLandPurchaseTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("LandPurchaseType must be 'CustomerHasLand' or 'CustomerWantsToPurchaseLand'.");

        // --- CustomerHasLand branch ---

        RuleFor(x => x.CustomerLandType)
            .NotEmpty()
            .Must(t => ValidCustomerLandTypes.Contains(t!, StringComparer.OrdinalIgnoreCase))
            .When(x => IsCustomerHasLand(x))
            .WithMessage("CustomerLandType must be 'CustomerOwnedLand', 'PrivateProperty', or 'CommunityOrNeighborhood'.");

        // CustomerOwnedLand → LandInclusion required
        RuleFor(x => x.LandInclusion)
            .NotEmpty()
            .Must(t => ValidLandInclusions.Contains(t!, StringComparer.OrdinalIgnoreCase))
            .When(x => IsCustomerOwnedLand(x))
            .WithMessage("LandInclusion must be 'CustomerLandPayoff', 'CustomerLandInLieu', or 'HomeOnly'.");

        // CustomerLandPayoff → 6 required financial fields
        RuleFor(x => x.EstimatedValue)
            .NotNull().GreaterThan(0)
            .When(x => IsCustomerLandPayoff(x))
            .WithMessage("EstimatedValue is required and must be greater than zero for CustomerLandPayoff.");

        RuleFor(x => x.SizeInAcres)
            .NotNull().GreaterThan(0)
            .When(x => IsCustomerLandPayoff(x))
            .WithMessage("SizeInAcres is required and must be greater than zero for CustomerLandPayoff.");

        RuleFor(x => x.PayoffAmountFinancing)
            .NotNull().GreaterThan(0)
            .When(x => IsCustomerLandPayoff(x))
            .WithMessage("PayoffAmountFinancing is required and must be greater than zero for CustomerLandPayoff.");

        RuleFor(x => x.LandEquity)
            .NotNull()
            .When(x => IsCustomerLandPayoff(x))
            .WithMessage("LandEquity is required for CustomerLandPayoff."); // Negative allowed (underwater land)

        RuleFor(x => x.OriginalPurchaseDate)
            .NotNull()
            .When(x => IsCustomerLandPayoff(x))
            .WithMessage("OriginalPurchaseDate is required for CustomerLandPayoff.");

        RuleFor(x => x.OriginalPurchasePrice)
            .NotNull().GreaterThan(0)
            .When(x => IsCustomerLandPayoff(x))
            .WithMessage("OriginalPurchasePrice is required and must be greater than zero for CustomerLandPayoff.");

        // PrivateProperty → optional lot rent and phone validation
        RuleFor(x => x.PropertyLotRent)
            .GreaterThan(0)
            .When(x => IsPrivateProperty(x) && x.PropertyLotRent.HasValue)
            .WithMessage("PropertyLotRent must be greater than zero when provided.");

        RuleFor(x => x.PropertyOwnerPhoneNumber)
            .Matches(@"^\d{10}$")
            .When(x => IsPrivateProperty(x) && !string.IsNullOrEmpty(x.PropertyOwnerPhoneNumber))
            .WithMessage("PropertyOwnerPhoneNumber must be exactly 10 digits.");

        // CommunityOrNeighborhood → required community fields
        RuleFor(x => x.CommunityName)
            .NotEmpty()
            .When(x => IsCommunityOrNeighborhood(x))
            .WithMessage("CommunityName is required for CommunityOrNeighborhood.");

        RuleFor(x => x.CommunityManagerName)
            .NotEmpty()
            .When(x => IsCommunityOrNeighborhood(x))
            .WithMessage("CommunityManagerName is required for CommunityOrNeighborhood.");

        RuleFor(x => x.CommunityManagerPhoneNumber)
            .NotEmpty()
            .When(x => IsCommunityOrNeighborhood(x))
            .WithMessage("CommunityManagerPhoneNumber is required for CommunityOrNeighborhood.");

        RuleFor(x => x.CommunityManagerEmail)
            .NotEmpty()
            .When(x => IsCommunityOrNeighborhood(x))
            .WithMessage("CommunityManagerEmail is required for CommunityOrNeighborhood.");

        RuleFor(x => x.CommunityMonthlyCost)
            .NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(1_000_000)
            .When(x => IsCommunityOrNeighborhood(x))
            .WithMessage("CommunityMonthlyCost is required and must be between 0 and 1,000,000 for CommunityOrNeighborhood.");

        // --- CustomerWantsToPurchaseLand branch ---

        RuleFor(x => x.TypeOfLandWanted)
            .NotEmpty()
            .Must(t => ValidTypesOfLandWanted.Contains(t!, StringComparer.OrdinalIgnoreCase))
            .When(x => IsCustomerWantsToPurchaseLand(x))
            .WithMessage("TypeOfLandWanted must be 'LandPurchase' or 'HomeCenterOwnedLand'.");

        // HomeCenterOwnedLand → stock number, costs, cross-validation
        RuleFor(x => x.LandStockNumber)
            .NotEmpty()
            .When(x => IsHomeCenterOwnedLand(x))
            .WithMessage("LandStockNumber is required for HomeCenterOwnedLand.");

        RuleFor(x => x.LandCost)
            .NotNull().GreaterThan(0)
            .When(x => IsHomeCenterOwnedLand(x))
            .WithMessage("LandCost is required and must be greater than zero for HomeCenterOwnedLand.");

        RuleFor(x => x.LandSalesPrice)
            .NotNull()
            .InclusiveBetween(0, 1_000_000)
            .When(x => IsHomeCenterOwnedLand(x))
            .WithMessage("LandSalesPrice must be between 0 and 1,000,000 for HomeCenterOwnedLand.");

        // Cross-validation: EstimatedCost must equal LandCost
        RuleFor(x => x.EstimatedCost)
            .Equal(x => x.LandCost!.Value)
            .When(x => IsHomeCenterOwnedLand(x) && x.LandCost.HasValue)
            .WithMessage("EstimatedCost must equal LandCost for HomeCenterOwnedLand.");

        // Cross-validation: SalePrice must equal LandSalesPrice
        RuleFor(x => x.SalePrice)
            .Equal(x => x.LandSalesPrice!.Value)
            .When(x => IsHomeCenterOwnedLand(x) && x.LandSalesPrice.HasValue)
            .WithMessage("SalePrice must equal LandSalesPrice for HomeCenterOwnedLand.");

        // LandPurchase → PurchasePrice required
        RuleFor(x => x.PurchasePrice)
            .NotNull()
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000)
            .When(x => IsLandPurchase(x))
            .WithMessage("PurchasePrice is required, must be greater than zero and at most 1,000,000 for LandPurchase.");
    }

    // --- Branch predicates ---

    private static bool IsCustomerHasLand(UpdatePackageLandCommand x) =>
        string.Equals(x.LandPurchaseType, "CustomerHasLand", StringComparison.OrdinalIgnoreCase);

    private static bool IsCustomerWantsToPurchaseLand(UpdatePackageLandCommand x) =>
        string.Equals(x.LandPurchaseType, "CustomerWantsToPurchaseLand", StringComparison.OrdinalIgnoreCase);

    private static bool IsCustomerOwnedLand(UpdatePackageLandCommand x) =>
        IsCustomerHasLand(x) &&
        string.Equals(x.CustomerLandType, "CustomerOwnedLand", StringComparison.OrdinalIgnoreCase);

    private static bool IsPrivateProperty(UpdatePackageLandCommand x) =>
        IsCustomerHasLand(x) &&
        string.Equals(x.CustomerLandType, "PrivateProperty", StringComparison.OrdinalIgnoreCase);

    private static bool IsCommunityOrNeighborhood(UpdatePackageLandCommand x) =>
        IsCustomerHasLand(x) &&
        string.Equals(x.CustomerLandType, "CommunityOrNeighborhood", StringComparison.OrdinalIgnoreCase);

    private static bool IsCustomerLandPayoff(UpdatePackageLandCommand x) =>
        IsCustomerOwnedLand(x) &&
        string.Equals(x.LandInclusion, "CustomerLandPayoff", StringComparison.OrdinalIgnoreCase);

    private static bool IsHomeCenterOwnedLand(UpdatePackageLandCommand x) =>
        IsCustomerWantsToPurchaseLand(x) &&
        string.Equals(x.TypeOfLandWanted, "HomeCenterOwnedLand", StringComparison.OrdinalIgnoreCase);

    private static bool IsLandPurchase(UpdatePackageLandCommand x) =>
        IsCustomerWantsToPurchaseLand(x) &&
        string.Equals(x.TypeOfLandWanted, "LandPurchase", StringComparison.OrdinalIgnoreCase);
}
