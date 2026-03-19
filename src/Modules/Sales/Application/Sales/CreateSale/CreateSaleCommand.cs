using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Sales.CreateSale;

public sealed record CreateSaleCommand(
    Guid CustomerPublicId,
    int HomeCenterNumber,
    SaleType SaleType = SaleType.B2C) : ICommand<CreateSaleResult>;

public sealed record CreateSaleResult(
    Guid PublicId,
    int SaleNumber);
