# Package Handler Refactor & Bug Fixes

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Refactor `UpdatePackageHomeCommandHandler` for readability (self-contained steps, no sub-methods) and fix three Package aggregate bugs (C-2, C-7, W-12).

**Architecture:** Each handler step becomes a single private method with all supporting logic (constants, records, mapping) inlined. Bug fixes are isolated to the Package aggregate and its EF configuration — no handler behavior changes beyond the refactor.

**Tech Stack:** .NET 10, EF Core 10, PostgreSQL 16 (Npgsql), xUnit, NSubstitute

---

## Task 1: Refactor UpdatePackageHomeCommandHandler

**Goal:** Every step in `Handle()` is a single self-contained private method. No sub-methods. Constants and helper types live next to the method that uses them. The public method reads as a clean 8-step flow.

**Files:**
- Modify: `src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommandHandler.cs`
- Test: `src/Modules/Sales/test/Application.Tests/Packages/UpdatePackageHomeCommandHandlerTests.cs` (run existing — no new tests needed for pure refactor)

### Step 1: Read the current handler and understand the step flow

Read: `src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommandHandler.cs`

Current structure issues:
- Step 2 has 3 loose variables (`existingHome`, `oldDetails`, `taxSnapshot`) spanning lines 40-42
- `TakeTaxSnapshot` is a separate private method + `TaxSnapshot` record is disconnected from its usage
- Step 4 calls two sub-methods: `ClearPreviouslyTitled()` and `RemoveInvalidProjectCosts()`
- Step 5 (`RecalculateWheelAndAxle`) is OK but constants (`WaRentalCategoryNumber` etc.) are scattered across the file
- `MapToHomeDetails` is a standalone method only used by Step 3
- Constants for different steps are interleaved (W&A constants, refurb constants, use tax constants all at class level)

### Step 2: Rewrite the handler

Replace the entire handler with the refactored version below. The `Handle()` method becomes:

```csharp
public async Task<Result<UpdatePackageHomeResult>> Handle(
    UpdatePackageHomeCommand request,
    CancellationToken cancellationToken)
{
    var home = request.Home;

    // Step 1: Load package with all lines + sale context
    var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
        request.PackagePublicId, cancellationToken);

    if (package is null)
    {
        return Result.Failure<UpdatePackageHomeResult>(
            PackageErrors.NotFoundByPublicId(request.PackagePublicId));
    }

    // Step 2: Snapshot existing state for change detection
    var (previousHomeType, taxSnapshot) = TakePreMutationSnapshot(package);

    // Step 3: Upsert home line (delete-then-insert — PUT semantics)
    var upsertResult = await UpsertHomeLine(package, home, cancellationToken);
    if (upsertResult.IsFailure)
    {
        return Result.Failure<UpdatePackageHomeResult>(upsertResult.Error);
    }

    // Step 4: Cascade home type change (conditional — only when type actually changed)
    CascadeHomeTypeChange(package, previousHomeType, home.HomeType);

    // Step 5: Recalculate W&A pricing (always — handler knows home changed)
    await RecalculateWheelAndAxlePricing(package, home, cancellationToken);

    // Step 6: Clear tax calculation errors (always)
    ClearTaxErrors(package);

    // Step 7: Detect tax changes and flag for recalculation (ALWAYS second-to-last)
    DetectAndFlagTaxChanges(package, taxSnapshot, home);

    // Step 8: Finalize — raise events, recalculate GP, persist
    package.Sale.RaiseSaleSummaryChanged();
    package.RecalculateGrossProfit();
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return new UpdatePackageHomeResult(
        package.GrossProfit,
        package.CommissionableGrossProfit,
        package.MustRecalculateTaxes);
}
```

**Private method: `TakePreMutationSnapshot`** — replaces 3 loose variables + the old `TakeTaxSnapshot` + the `TaxSnapshot` record. The record is now a nested type immediately above this method:

```csharp
// --- Step 2: Snapshot ---

private sealed record TaxSnapshot(
    HomeType? HomeType,
    string? StockNumber,
    decimal HomeSalePrice,
    int ProjectCostCount,
    List<decimal> ProjectCostPrices);

private static (HomeType? PreviousHomeType, TaxSnapshot Snapshot) TakePreMutationSnapshot(
    Package package)
{
    var existingHome = package.Lines.OfType<HomeLine>().SingleOrDefault();

    var snapshot = new TaxSnapshot(
        HomeType: existingHome?.Details?.HomeType,
        StockNumber: existingHome?.Details?.StockNumber,
        HomeSalePrice: existingHome?.SalePrice ?? 0m,
        ProjectCostCount: package.Lines.OfType<ProjectCostLine>().Count(),
        ProjectCostPrices: package.Lines
            .OfType<ProjectCostLine>()
            .Select(l => l.SalePrice)
            .OrderBy(p => p)
            .ToList());

    return (existingHome?.Details?.HomeType, snapshot);
}
```

**Private method: `UpsertHomeLine`** — replaces Step 3 inline code + the old `MapToHomeDetails` method. All mapping lives inside this method:

```csharp
// --- Step 3: Upsert ---

private async Task<Result> UpsertHomeLine(
    Package package, UpdatePackageHomeRequest home, CancellationToken ct)
{
    package.RemoveLine<HomeLine>();

    // Resolve inventory cache FK for OnLot homes
    int? onLotHomeId = null;
    if (home.HomeSourceType is HomeSourceType.OnLot)
    {
        var homeCenterNumber = package.Sale.RetailLocation.RefHomeCenterNumber!.Value;
        var cached = await inventoryCacheQueries.FindByHomeCenterAndStockAsync(
            homeCenterNumber, home.StockNumber!, ct);

        if (cached is null)
        {
            return Result.Failure(Error.NotFound(
                "OnLotHome.NotFound",
                $"On-lot home with stock number '{home.StockNumber}' not found in inventory cache for home center {homeCenterNumber}."));
        }

        onLotHomeId = cached.Id;
    }

    // Map request → domain details (inline — only used here)
    var details = HomeDetails.Create(
        homeType: home.HomeType,
        homeSourceType: home.HomeSourceType,
        stockNumber: home.StockNumber,
        modularType: home.ModularType,
        vendor: home.Vendor,
        make: home.Make,
        model: home.Model,
        modelYear: home.ModelYear,
        lengthInFeet: home.LengthInFeet,
        widthInFeet: home.WidthInFeet,
        bedrooms: home.Bedrooms,
        bathrooms: home.Bathrooms,
        squareFootage: home.SquareFootage,
        serialNumbers: home.SerialNumbers,
        baseCost: home.BaseCost,
        optionsCost: home.OptionsCost,
        freightCost: home.FreightCost,
        invoiceCost: home.InvoiceCost,
        netInvoice: home.NetInvoice,
        grossCost: home.GrossCost,
        taxIncludedOnInvoice: home.TaxIncludedOnInvoice,
        numberOfWheels: home.NumberOfWheels,
        numberOfAxles: home.NumberOfAxles,
        wheelAndAxlesOption: home.WheelAndAxlesOption,
        numberOfFloorSections: home.NumberOfFloorSections,
        carrierFrameDeposit: home.CarrierFrameDeposit,
        rebateOnMfgInvoice: home.RebateOnMfgInvoice,
        claytonBuilt: home.ClaytonBuilt,
        buildType: home.BuildType,
        inventoryReferenceId: home.InventoryReferenceId,
        stateAssociationAndMhiDues: home.StateAssociationAndMhiDues,
        partnerAssistance: home.PartnerAssistance,
        distanceMiles: home.DistanceMiles,
        propertyType: home.PropertyType,
        purchaseOption: home.PurchaseOption,
        listingPrice: home.ListingPrice,
        accountNumber: home.AccountNumber,
        displayAccountId: home.DisplayAccountId,
        streetAddress: home.StreetAddress,
        city: home.City,
        state: home.State,
        zipCode: home.ZipCode);

    package.AddLine(HomeLine.Create(
        packageId: package.Id,
        salePrice: home.SalePrice,
        estimatedCost: home.EstimatedCost,
        retailSalePrice: home.RetailSalePrice,
        responsibility: Responsibility.Seller,
        details: details,
        onLotHomeId: onLotHomeId));

    return Result.Success();
}
```

**Private method: `CascadeHomeTypeChange`** — replaces `ClearPreviouslyTitled()` + `RemoveInvalidProjectCosts()`. All constants inline:

```csharp
// --- Step 4: Home type cascade ---

private static void CascadeHomeTypeChange(
    Package package, HomeType? previousHomeType, HomeType newHomeType)
{
    if (previousHomeType is null || previousHomeType == newHomeType)
    {
        return;
    }

    // Clear PreviouslyTitled on tax line (home-type-specific tax field)
    var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
    taxLine?.ClearPreviouslyTitled();

    // W&A project costs will be recalculated in Step 5 — remove them now
    const int waRentalCat = 1, waRentalItem = 28;
    const int waPurchaseCat = 1, waPurchaseItem = 29;
    package.RemoveProjectCost(waRentalCat, waRentalItem);
    package.RemoveProjectCost(waPurchaseCat, waPurchaseItem);

    // Remove project costs invalid for the new home type
    // Legacy: PackageDtoExtensions.GetItemsToRemoveByHomeType()
    const int refurbCat = 11, cleaningItem = 1, repairItem = 2, partsItem = 3, drapesItem = 4;
    const int repoCostsCat = 12;
    const int miscTaxCat = 13, taxUndercollectionItem = 98;
    const int decoratingCat = 15;

    switch (newHomeType)
    {
        case HomeType.New:
            package.RemoveProjectCost(refurbCat, cleaningItem);
            package.RemoveProjectCost(refurbCat, repairItem);
            package.RemoveProjectCost(refurbCat, partsItem);
            package.RemoveProjectCost(refurbCat, drapesItem);
            package.RemoveProjectCostsByCategory(repoCostsCat);
            package.RemoveProjectCost(miscTaxCat, taxUndercollectionItem);
            break;
        case HomeType.Used:
            package.RemoveProjectCostsByCategory(repoCostsCat);
            package.RemoveProjectCost(decoratingCat, drapesItem);
            package.RemoveProjectCost(miscTaxCat, taxUndercollectionItem);
            break;
        case HomeType.Repo:
            package.RemoveProjectCost(refurbCat, cleaningItem);
            package.RemoveProjectCost(refurbCat, repairItem);
            package.RemoveProjectCost(refurbCat, partsItem);
            package.RemoveProjectCost(decoratingCat, drapesItem);
            package.RemoveProjectCost(miscTaxCat, taxUndercollectionItem);
            break;
    }
}
```

**Private method: `RecalculateWheelAndAxlePricing`** — same logic, constants inline:

```csharp
// --- Step 5: W&A pricing ---

private async Task RecalculateWheelAndAxlePricing(
    Package package, UpdatePackageHomeRequest home, CancellationToken ct)
{
    const int waRentalCat = 1, waRentalItem = 28;
    const int waPurchaseCat = 1, waPurchaseItem = 29;

    // Always remove existing W&A project costs
    package.RemoveProjectCost(waRentalCat, waRentalItem);
    package.RemoveProjectCost(waPurchaseCat, waPurchaseItem);

    if (home.WheelAndAxlesOption is null)
    {
        return;
    }

    var (catId, itemId) = home.WheelAndAxlesOption.Value switch
    {
        WheelAndAxlesOption.Rent => (waRentalCat, waRentalItem),
        WheelAndAxlesOption.Purchase => (waPurchaseCat, waPurchaseItem),
        _ => (waRentalCat, waRentalItem)
    };

    // Calculate W&A price via iSeries — stock number path or wheel/axle count path
    WheelAndAxlePriceResult waResult;
    if (home.HomeSourceType is HomeSourceType.OnLot or HomeSourceType.VmfHomes
        && !string.IsNullOrEmpty(home.StockNumber))
    {
        var homeCenterNumber = package.Sale.RetailLocation.RefHomeCenterNumber ?? 0;
        waResult = await iSeriesAdapter.GetWheelAndAxlePriceByStock(
            new WheelAndAxlePriceByStockRequest
            {
                HomeCenterNumber = homeCenterNumber,
                StockNumber = home.StockNumber
            }, ct);
    }
    else if (home.NumberOfWheels.HasValue && home.NumberOfAxles.HasValue)
    {
        waResult = await iSeriesAdapter.CalculateWheelAndAxlePriceByCount(
            new WheelAndAxlePriceByCountRequest
            {
                NumberOfWheels = home.NumberOfWheels.Value,
                NumberOfAxles = home.NumberOfAxles.Value
            }, ct);
    }
    else
    {
        return;
    }

    if (waResult.SalePrice <= 0)
    {
        return;
    }

    var details = ProjectCostDetails.Create(
        categoryId: catId,
        itemId: itemId,
        itemDescription: home.WheelAndAxlesOption.Value == WheelAndAxlesOption.Rent
            ? "Wheels & Axles - Rental"
            : "Wheels & Axles - Purchase");

    package.AddLine(ProjectCostLine.Create(
        packageId: package.Id,
        salePrice: waResult.SalePrice,
        estimatedCost: waResult.Cost,
        retailSalePrice: waResult.SalePrice,
        responsibility: Responsibility.Seller,
        shouldExcludeFromPricing: false,
        details: details));
}
```

**Private method: `ClearTaxErrors`** — unchanged (already clean and self-contained):

```csharp
// --- Step 6: Tax errors ---

private static void ClearTaxErrors(Package package)
{
    var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
    if (taxLine?.Details?.Errors is null or [])
    {
        return;
    }

    taxLine.ClearErrors();
}
```

**Private method: `DetectAndFlagTaxChanges`** — replaces `DetectTaxChanges`, constants inline, no longer async:

```csharp
// --- Step 7: Tax change detection ---

private static void DetectAndFlagTaxChanges(
    Package package, TaxSnapshot before, UpdatePackageHomeRequest home)
{
    const int useTaxCat = 9, useTaxItem = 21;

    var currentProjectCosts = package.Lines.OfType<ProjectCostLine>().ToList();

    var changed = before.HomeType != home.HomeType
        || !string.Equals(before.StockNumber, home.StockNumber, StringComparison.OrdinalIgnoreCase)
        || before.HomeSalePrice != home.SalePrice
        || before.ProjectCostCount != currentProjectCosts.Count
        || !before.ProjectCostPrices.SequenceEqual(
            currentProjectCosts.Select(l => l.SalePrice).OrderBy(p => p));

    if (!changed)
    {
        return;
    }

    var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
    taxLine?.ClearCalculations();

    package.RemoveProjectCost(useTaxCat, useTaxItem);
    package.FlagForTaxRecalculation();
}
```

**What was deleted:** `ClearPreviouslyTitled()`, `RemoveInvalidProjectCosts()`, `MapToHomeDetails()`, all class-level constants. Everything now lives inside the step method that uses it.

### Step 3: Run existing tests

Run: `dotnet test src/Modules/Sales/test/Application.Tests --filter "UpdatePackageHomeCommandHandlerTests" -v n`
Expected: All 3 tests pass (Returns_failure_when_package_not_found, Returns_failure_when_on_lot_home_not_found_in_inventory_cache, Returns_success_on_happy_path)

### Step 4: Run full Sales domain + application test suite

Run: `dotnet test src/Modules/Sales/test/Domain.Tests -v n && dotnet test src/Modules/Sales/test/Application.Tests -v n`
Expected: All tests pass

### Step 5: Commit

```bash
git add src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommandHandler.cs
git commit -m "refactor: make UpdatePackageHomeCommandHandler steps self-contained

Each step is now a single private method with all constants, records,
and mapping logic inlined. No sub-methods. Handle() reads as a clean
8-step flow."
```

---

## Task 2: Fix W-12 — Domain events on line removal

**Goal:** `RemoveLine<T>()` raises the same domain events as `AddLine()` for HomeLine and LandLine. Currently asymmetric — AddLine raises events, RemoveLine does not.

**Impact:** If a handler removes a HomeLine without replacement, `HomeLineUpdatedDomainEventHandler` never fires, so HomeFirst insurance and warranty lines aren't cleaned up.

**Files:**
- Modify: `src/Modules/Sales/Domain/Packages/Package.cs`
- Test: `src/Modules/Sales/test/Domain.Tests/Packages/PackageLineRemovalTests.cs`

### Step 1: Write the failing tests

Add to `PackageLineRemovalTests.cs`:

```csharp
[Fact]
public void RemoveHomeLine_raises_HomeLineUpdatedDomainEvent()
{
    var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
    package.AddLine(HomeLine.Create(package.Id, 100_000m, 80_000m, 100_000m, Responsibility.Seller, details: null));
    package.ClearDomainEvents();

    package.RemoveLine<HomeLine>();

    Assert.Contains(package.DomainEvents, e => e is HomeLineUpdatedDomainEvent);
}

[Fact]
public void RemoveHomeLine_does_not_raise_event_when_absent()
{
    var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
    package.ClearDomainEvents();

    package.RemoveLine<HomeLine>();

    Assert.DoesNotContain(package.DomainEvents, e => e is HomeLineUpdatedDomainEvent);
}

[Fact]
public void RemoveLandLine_raises_LandLineUpdatedDomainEvent()
{
    var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
    package.AddLine(LandLine.Create(package.Id, 50_000m, 40_000m, 50_000m, Responsibility.Seller, details: null));
    package.ClearDomainEvents();

    package.RemoveLine<LandLine>();

    Assert.Contains(package.DomainEvents, e => e is LandLineUpdatedDomainEvent);
}

[Fact]
public void RemoveLandLine_does_not_raise_event_when_absent()
{
    var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
    package.ClearDomainEvents();

    package.RemoveLine<LandLine>();

    Assert.DoesNotContain(package.DomainEvents, e => e is LandLineUpdatedDomainEvent);
}
```

Add required `using` if not present:
```csharp
using Modules.Sales.Domain.Packages.Events;
```

### Step 2: Run tests to verify they fail

Run: `dotnet test src/Modules/Sales/test/Domain.Tests --filter "RemoveHomeLine_raises|RemoveLandLine_raises|does_not_raise" -v n`
Expected: 2 FAIL (the "raises" tests), 2 PASS (the "does not raise when absent" tests)

### Step 3: Implement the fix

Modify `Package.cs` — update `RemoveLine<T>()` to raise domain events when HomeLine or LandLine is removed:

```csharp
public T? RemoveLine<T>() where T : PackageLine
{
    var line = _lines
        .OfType<T>()
        .SingleOrDefault();

    if (line is not null)
    {
        _lines.Remove(line);

        if (line is HomeLine)
        {
            Raise(new HomeLineUpdatedDomainEvent { PackageId = Id, SaleId = SaleId });
        }
        else if (line is LandLine)
        {
            Raise(new LandLineUpdatedDomainEvent { PackageId = Id, SaleId = SaleId });
        }
    }

    return line;
}
```

### Step 4: Run tests to verify they pass

Run: `dotnet test src/Modules/Sales/test/Domain.Tests --filter "PackageLineRemovalTests" -v n`
Expected: All tests pass (existing + new)

### Step 5: Commit

```bash
git add src/Modules/Sales/Domain/Packages/Package.cs src/Modules/Sales/test/Domain.Tests/Packages/PackageLineRemovalTests.cs
git commit -m "fix(C-W12): raise domain events on HomeLine/LandLine removal

RemoveLine<T>() now raises HomeLineUpdatedDomainEvent and
LandLineUpdatedDomainEvent, matching the symmetry with AddLine().
Ensures insurance/warranty/tax handlers fire on standalone removals."
```

---

## Task 3: Fix C-7 — PostgreSQL xmin concurrency token

**Goal:** Replace the non-functional `int? Version` property with PostgreSQL's built-in `xmin` system column. `xmin` auto-updates on every row modification — no domain code needed.

**Current problem:** `Version` is configured as `.IsConcurrencyToken()` but is nullable, never incremented, and starts as null. `WHERE version IS NULL` always matches, so concurrent writes silently overwrite each other.

**Files:**
- Modify: `src/Modules/Sales/Domain/Packages/Package.cs` — remove `Version` property
- Modify: `src/Modules/Sales/Infrastructure/Persistence/Configurations/PackageConfiguration.cs` — switch to xmin
- Create: new EF Core migration (drop `version` column)
- Test: `src/Modules/Sales/test/Domain.Tests/Packages/PackageLineRemovalTests.cs` — verify no compilation errors

### Step 1: Remove Version property from Package.cs

In `Package.cs`, delete:
```csharp
public int? Version { get; private set; } // Optimistic concurrency
```

And update the class comment (line 17) to remove the "Version provides optimistic concurrency" mention:
```csharp
// Entity — packages.packages. A home package containing all pricing lines for a sale.
// Packages are ranked; the primary package (Ranking == 1) is the active one.
// PublicId (UUID v7) used in API routes. Concurrency protected via PostgreSQL xmin.
```

### Step 2: Update PackageConfiguration.cs

Replace the Version configuration with `UseXminAsConcurrencyToken()`:

Delete:
```csharp
builder.Property(p => p.Version)
    .HasColumnName("version")
    .IsConcurrencyToken();
```

Add (after the `PublicId` configuration):
```csharp
builder.UseXminAsConcurrencyToken();
```

`UseXminAsConcurrencyToken()` is a Npgsql extension method that:
- Adds a shadow property `xmin` of type `uint`
- Configures it as a concurrency token
- PostgreSQL automatically updates `xmin` on every row modification
- EF Core includes `WHERE xmin = @loaded_value` in UPDATE/DELETE statements
- If the row was modified between load and save, `DbUpdateConcurrencyException` is thrown

### Step 3: Verify compilation

Run: `dotnet build src/Modules/Sales/Domain && dotnet build src/Modules/Sales/Infrastructure`
Expected: Build succeeds. If any code references `Package.Version`, fix those references (search confirmed: no external references exist).

### Step 4: Generate migration

**Note:** Migration regeneration requires `ENCRYPTION_KEY` env var per project convention.

```bash
export ENCRYPTION_KEY=$(openssl rand -base64 32)
cd /x/SES/223-MM-template/Modular-Template
dotnet ef migrations add RemoveVersionUseXmin \
    --project src/Modules/Sales/Infrastructure \
    --startup-project src/Api/Host \
    --context SalesDbContext \
    -- --environment Development
```

The generated migration should contain:
```csharp
migrationBuilder.DropColumn(
    name: "version",
    schema: "packages",
    table: "packages");
```

No column is added for `xmin` — it's a PostgreSQL system column that exists on every row automatically.

### Step 5: Run tests

Run: `dotnet test src/Modules/Sales/test/Domain.Tests -v n && dotnet test src/Modules/Sales/test/Application.Tests -v n`
Expected: All tests pass

### Step 6: Commit

```bash
git add src/Modules/Sales/Domain/Packages/Package.cs \
        src/Modules/Sales/Infrastructure/Persistence/Configurations/PackageConfiguration.cs \
        src/Modules/Sales/Infrastructure/Persistence/Migrations/
git commit -m "fix(C-7): replace dead Version token with PostgreSQL xmin concurrency

Version was configured as IsConcurrencyToken but never incremented,
making concurrent write protection non-functional. PostgreSQL xmin
auto-updates on every row change — no domain code needed."
```

---

## Task 4: Fix C-2 — Add MustRecalculateCommission flag

**Goal:** Signal that `CommissionableGrossProfit` is stale after any line mutation changes `GrossProfit`. Add a `MustRecalculateCommission` boolean flag to Package, analogous to `MustRecalculateTaxes`.

**Current state:** The audit's C-2 described `CommissionableGrossProfit = grossProfit;` inside `RecalculateGrossProfit()`, but the current code does NOT have that destructive line — the overwrite was already removed. The remaining issue is: after GP changes, there's no signal that commission needs recalculation. The API returns a potentially stale `CommissionableGrossProfit` with no warning.

**Files:**
- Modify: `src/Modules/Sales/Domain/Packages/Package.cs`
- Modify: `src/Modules/Sales/Infrastructure/Persistence/Configurations/PackageConfiguration.cs`
- Modify: `src/Modules/Sales/Application/Commission/CalculateCommission/CalculateCommissionCommandHandler.cs`
- Modify: `src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommand.cs` (result type)
- Create: new EF Core migration
- Test: `src/Modules/Sales/test/Domain.Tests/Packages/PackageLineRemovalTests.cs` (new tests)

### Step 1: Write failing tests

Add to `PackageLineRemovalTests.cs`:

```csharp
[Fact]
public void RecalculateGrossProfit_flags_commission_for_recalculation_when_gp_changes()
{
    var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
    package.AddLine(HomeLine.Create(package.Id, 100_000m, 80_000m, 100_000m, Responsibility.Seller, details: null));
    package.RecalculateGrossProfit();

    // Simulate commission calculation
    package.SetCommissionableGrossProfit(18_000m);
    package.ClearCommissionRecalculationFlag();
    Assert.False(package.MustRecalculateCommission);

    // Add a project cost — changes GP
    package.AddLine(ProjectCostLine.Create(package.Id, 500m, 300m, 500m, Responsibility.Seller,
        shouldExcludeFromPricing: false, details: ProjectCostDetails.Create(9, 21, "Use Tax")));
    package.RecalculateGrossProfit();

    Assert.True(package.MustRecalculateCommission);
}

[Fact]
public void RecalculateGrossProfit_does_not_flag_commission_when_gp_unchanged()
{
    var package = Package.Create(saleId: 1, name: "Pkg", isPrimary: true);
    package.AddLine(HomeLine.Create(package.Id, 100_000m, 80_000m, 100_000m, Responsibility.Seller, details: null));
    package.RecalculateGrossProfit();
    package.ClearCommissionRecalculationFlag();

    // Recalculate again with same lines — GP unchanged
    package.RecalculateGrossProfit();

    Assert.False(package.MustRecalculateCommission);
}
```

### Step 2: Run tests to verify they fail

Run: `dotnet test src/Modules/Sales/test/Domain.Tests --filter "commission" -v n`
Expected: FAIL — `MustRecalculateCommission` and `ClearCommissionRecalculationFlag` don't exist

### Step 3: Add property and methods to Package.cs

Add after `MustRecalculateTaxes` (line 32):

```csharp
public bool MustRecalculateCommission { get; private set; }
```

Add methods after `ClearTaxRecalculationFlag()` (line 71):

```csharp
public void ClearCommissionRecalculationFlag() => MustRecalculateCommission = false;
```

Modify `RecalculateGrossProfit()` to detect GP changes:

```csharp
public void RecalculateGrossProfit()
{
    var newGrossProfit = _lines
        .Where(l => !l.ShouldExcludeFromPricing)
        .Sum(l => l.SalePrice - l.EstimatedCost);

    if (newGrossProfit != GrossProfit)
    {
        MustRecalculateCommission = true;
    }

    GrossProfit = newGrossProfit;
}
```

### Step 4: Run tests to verify they pass

Run: `dotnet test src/Modules/Sales/test/Domain.Tests --filter "PackageLineRemovalTests" -v n`
Expected: All tests pass

### Step 5: Add EF configuration

In `PackageConfiguration.cs`, add after `MustRecalculateTaxes`:

```csharp
builder.Property(p => p.MustRecalculateCommission)
    .HasColumnName("must_recalculate_commission");
```

### Step 6: Update CalculateCommissionCommandHandler to clear the flag

In `CalculateCommissionCommandHandler.cs`, after line 215 (`package.SetCommissionableGrossProfit(...)`), add:

```csharp
package.ClearCommissionRecalculationFlag();
```

### Step 7: Update the result type

In `UpdatePackageHomeCommand.cs`, update `UpdatePackageHomeResult`:

```csharp
public sealed record UpdatePackageHomeResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes,
    bool MustRecalculateCommission);
```

Update `Handle()` return in the refactored handler (Step 8):

```csharp
return new UpdatePackageHomeResult(
    package.GrossProfit,
    package.CommissionableGrossProfit,
    package.MustRecalculateTaxes,
    package.MustRecalculateCommission);
```

**Note:** Other handlers that call `RecalculateGrossProfit()` and return result types with GP data should also return `MustRecalculateCommission`. This can be done incrementally — the flag is persisted to the database regardless.

### Step 8: Generate migration

```bash
export ENCRYPTION_KEY=$(openssl rand -base64 32)
cd /x/SES/223-MM-template/Modular-Template
dotnet ef migrations add AddMustRecalculateCommission \
    --project src/Modules/Sales/Infrastructure \
    --startup-project src/Api/Host \
    --context SalesDbContext \
    -- --environment Development
```

Expected migration:
```csharp
migrationBuilder.AddColumn<bool>(
    name: "must_recalculate_commission",
    schema: "packages",
    table: "packages",
    nullable: false,
    defaultValue: false);
```

### Step 9: Run full test suite

Run: `dotnet test src/Modules/Sales/test/Domain.Tests -v n && dotnet test src/Modules/Sales/test/Application.Tests -v n`
Expected: All tests pass. `UpdatePackageHomeCommandHandlerTests` may need the result assertion updated for the new field.

### Step 10: Commit

```bash
git add src/Modules/Sales/Domain/Packages/Package.cs \
        src/Modules/Sales/Infrastructure/Persistence/Configurations/PackageConfiguration.cs \
        src/Modules/Sales/Application/Commission/CalculateCommission/CalculateCommissionCommandHandler.cs \
        src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommand.cs \
        src/Modules/Sales/Application/Packages/UpdatePackageHome/UpdatePackageHomeCommandHandler.cs \
        src/Modules/Sales/Infrastructure/Persistence/Migrations/ \
        src/Modules/Sales/test/Domain.Tests/Packages/PackageLineRemovalTests.cs
git commit -m "fix(C-2): add MustRecalculateCommission flag to Package

RecalculateGrossProfit() now sets MustRecalculateCommission when GP
changes. CalculateCommissionCommandHandler clears the flag after
computing fresh CommissionableGrossProfit. API responses include the
flag so clients know when commission data is stale."
```

---

## Task 5: Update audit document

**Files:**
- Modify: `src/Modules/Sales/PACKAGE-DOMAIN-AUDIT.md`

Mark C-2, C-7, and W-12 as **FIXED** with date and brief description of the fix.

### Step 1: Update audit entries

For C-2 (line 23): Add `**Status:** Fixed — removed destructive overwrite, added `MustRecalculateCommission` flag.`

For C-7 (line 124): Add `**Status:** Fixed — replaced dead `Version` property with PostgreSQL `xmin` via `UseXminAsConcurrencyToken()`.`

For W-12 (line 165): Add `**Status:** Fixed — `RemoveLine<T>()` now raises `HomeLineUpdatedDomainEvent` / `LandLineUpdatedDomainEvent`, symmetric with `AddLine()`.`

### Step 2: Commit

```bash
git add src/Modules/Sales/PACKAGE-DOMAIN-AUDIT.md
git commit -m "docs: mark C-2, C-7, W-12 as fixed in package audit"
```
