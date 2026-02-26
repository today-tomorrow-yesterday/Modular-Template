using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Packages.Tax;
using Modules.Sales.Domain.Packages.TradeIns;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageTradeIns;

// Flow: PUT /api/v1/packages/{packageId}/trade-ins → UpdatePackageTradeInsCommand →
//   replace all trade-in lines + Trade Over Allowance project cost sync +
//   tax change detection + recalculate gross profit →
//   returns updated GP/CGP/MustRecalculateTaxes
internal sealed class UpdatePackageTradeInsCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageTradeInsCommand, UpdatePackageTradeInsResult>
{
    public async Task<Result<UpdatePackageTradeInsResult>> Handle(
        UpdatePackageTradeInsCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageTradeInsResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Capture pre-update snapshot for tax change detection
        var oldTradeInPrices = package.Lines
            .OfType<TradeInLine>()
            .Select(l => l.SalePrice)
            .OrderBy(p => p)
            .ToList();
        var oldProjectCostCount = package.Lines.Count(l => !l.ShouldExcludeFromPricing);
        var oldProjectCostPrices = package.Lines
            .Where(l => !l.ShouldExcludeFromPricing)
            .Select(l => l.SalePrice)
            .OrderBy(p => p)
            .ToList();

        // Step 3: Replace all TradeIn lines (PUT semantics — delete all old, insert new set)
        package.RemoveAllLines<TradeInLine>();

        for (var i = 0; i < request.Items.Length; i++)
        {
            var item = request.Items[i];

            var details = TradeInDetails.Create(
                tradeType: item.TradeType,
                year: item.Year,
                make: item.Make,
                model: item.Model,
                tradeAllowance: item.TradeAllowance,
                payoffAmount: item.PayoffAmount,
                bookInAmount: item.BookInAmount,
                floorWidth: item.FloorWidth,
                floorLength: item.FloorLength);

            var line = TradeInLine.Create(
                packageId: package.Id,
                salePrice: item.SalePrice,
                estimatedCost: item.EstimatedCost,
                retailSalePrice: item.RetailSalePrice,
                responsibility: Responsibility.Buyer,
                details: details,
                sortOrder: i);

            package.AddLine(line);
        }

        // Step 4: Trade Over Allowance project cost sync (Cat 10, Item 9)
        SyncTradeOverAllowance(package, request.Items);

        // Step 5: Tax change detection — compare old vs current state
        DetectTaxChanges(
            package, oldTradeInPrices, oldProjectCostCount, oldProjectCostPrices);

        // Step 6: Recalculate GP after all mutations complete
        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageTradeInsResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    private static void SyncTradeOverAllowance(
        Package package, UpdatePackageTradeInItemRequest[] items)
    {
        // Remove ALL existing Trade Over Allowance project costs — starting fresh
        package.RemoveAllProjectCosts(ProjectCostCategories.TradeOverAllowance, ProjectCostItems.TradeOverAllowance);

        // For each trade-in where TradeAllowance > BookInAmount, create a Trade Over Allowance PC
        foreach (var item in items)
        {
            var overAllowance = item.TradeAllowance - item.BookInAmount;
            if (overAllowance <= 0) continue;

            var details = ProjectCostDetails.Create(
                categoryId: ProjectCostCategories.TradeOverAllowance,
                itemId: ProjectCostItems.TradeOverAllowance,
                itemDescription: "Trade Over Allowance");

            package.AddLine(ProjectCostLine.Create(
                packageId: package.Id,
                salePrice: 0m,
                estimatedCost: overAllowance,
                retailSalePrice: 0m,
                responsibility: Responsibility.Seller,
                shouldExcludeFromPricing: false,
                details: details));
        }
    }

    private static void DetectTaxChanges(
        Package package,
        List<decimal> oldTradeInPrices,
        int oldProjectCostCount,
        List<decimal> oldProjectCostPrices)
    {
        var newTradeInPrices = package.Lines
            .OfType<TradeInLine>()
            .Select(l => l.SalePrice)
            .OrderBy(p => p)
            .ToList();
        var newProjectCostCount = package.Lines.Count(l => !l.ShouldExcludeFromPricing);
        var newProjectCostPrices = package.Lines
            .Where(l => !l.ShouldExcludeFromPricing)
            .Select(l => l.SalePrice)
            .OrderBy(p => p)
            .ToList();

        var changed = !oldTradeInPrices.SequenceEqual(newTradeInPrices)
            || oldProjectCostCount != newProjectCostCount
            || !oldProjectCostPrices.SequenceEqual(newProjectCostPrices);

        if (!changed) return;

        // Clear existing tax calculation results
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        taxLine?.ClearCalculations();

        // Remove Use Tax project cost (Cat 9, Item 21)
        package.RemoveProjectCost(ProjectCostCategories.UseTax, ProjectCostItems.UseTax);

        // Signal that taxes must be recalculated
        package.FlagForTaxRecalculation();
    }

}
