using Microsoft.Extensions.Logging;
using Modules.Sales.Domain.Packages.Events;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.EventHandlers;

// TODO: Send notification email to the salesperson(s) on the affected package.
// TODO: Determine recipient — primary salesperson? All team members? Sale owner?
// TODO: Determine email template and content.
internal sealed class ProductRemovedFromInventoryDomainEventHandler(
    ILogger<ProductRemovedFromInventoryDomainEventHandler> logger)
    : DomainEventHandler<ProductRemovedFromInventoryDomainEvent>
{
    public override Task Handle(
        ProductRemovedFromInventoryDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Product removed from inventory: {ProductType} (StockNumber={StockNumber}) " +
            "on Package {PackagePublicId} (Sale {SaleId}, Line {PackageLineId}). " +
            "Salesperson notification pending implementation.",
            domainEvent.ProductType,
            domainEvent.StockNumber,
            domainEvent.PackagePublicId,
            domainEvent.SaleId,
            domainEvent.PackageLineId);

        return Task.CompletedTask;
    }
}
