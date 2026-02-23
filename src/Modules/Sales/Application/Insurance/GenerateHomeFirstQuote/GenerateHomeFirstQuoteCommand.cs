using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Insurance.GenerateHomeFirstQuote;

public sealed record GenerateHomeFirstQuoteCommand(
    Guid SalePublicId,
    decimal CoverageAmount,
    char OccupancyType,
    bool IsHomeLocatedInPark,
    bool IsLandCustomerOwned,
    bool IsHomeOnPermanentFoundation,
    bool IsPremiumFinanced,
    DateTime CustomerBirthDate,
    DateTime? CoApplicantBirthDate,
    string MailingAddress,
    string MailingCity,
    string MailingState,
    string MailingZip) : ICommand<HomeFirstQuoteResult>;

public sealed record HomeFirstQuoteResult(
    int TempLinkId,
    string InsuranceCompanyName,
    decimal Premium,
    decimal CoverageAmount,
    decimal MaxCoverage,
    bool IsEligible,
    string? ErrorMessage);
