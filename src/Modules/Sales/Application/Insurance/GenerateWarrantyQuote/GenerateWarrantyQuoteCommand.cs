using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Insurance.GenerateWarrantyQuote;

public sealed record GenerateWarrantyQuoteCommand(
    Guid SalePublicId) : ICommand<GenerateWarrantyQuoteResult>;

public sealed record GenerateWarrantyQuoteResult(
    decimal Premium,
    decimal SalesTaxPremium,
    bool WarrantySelected);
