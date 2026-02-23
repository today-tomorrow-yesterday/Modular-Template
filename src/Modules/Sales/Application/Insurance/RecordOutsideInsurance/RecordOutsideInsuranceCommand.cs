using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Insurance.RecordOutsideInsurance;

public sealed record RecordOutsideInsuranceCommand(
    Guid SalePublicId,
    string ProviderName,
    decimal CoverageAmount,
    decimal PremiumAmount) : ICommand;
