using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Commission;
using Rtl.Core.Application.Adapters.ISeries.Insurance;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Adapters.ISeries.Tax;
using Rtl.Core.Infrastructure.ISeries.WireModels.Commission;
using Rtl.Core.Infrastructure.ISeries.WireModels.Insurance;
using Rtl.Core.Infrastructure.ISeries.WireModels.Pricing;
using Rtl.Core.Infrastructure.ISeries.WireModels.Tax;
using System.Diagnostics;

namespace Rtl.Core.Infrastructure.ISeries.Mapping;

internal static class WireMappingExtensions
{
    internal const int MasterDealerNumber = 29;

    internal static char ToChar(this HomeCondition c) => c switch
    {
        HomeCondition.New => 'N',
        HomeCondition.Used => 'U',
        HomeCondition.Repo => 'R',
        _ => throw new UnreachableException()
    };

    internal static string ToConditionString(this HomeCondition c) => c switch
    {
        HomeCondition.New => "New",
        HomeCondition.Used => "Used",
        HomeCondition.Repo => "Repo",
        _ => throw new UnreachableException()
    };

    internal static char ToModCodeChar(this ModularClassification m) => m switch
    {
        ModularClassification.Hud => 'N', // iSeries RPG expects 'N' (None) for HUD — not 'H'
        ModularClassification.OnFrame => 'F',
        ModularClassification.OffFrame => 'O',
        _ => throw new UnreachableException()
    };

    internal static string ToHudOrModString(this ModularClassification m) => m switch
    {
        ModularClassification.Hud => "HUD",
        ModularClassification.OnFrame => "On Frame",
        ModularClassification.OffFrame => "Off Frame",
        _ => throw new UnreachableException()
    };

    internal static char ToChar(this OccupancyType o) => o switch
    {
        OccupancyType.Primary => 'P',
        OccupancyType.Secondary => 'S',
        OccupancyType.Seasonal => 'E',
        OccupancyType.Rental => 'R',
        _ => throw new UnreachableException()
    };

    internal static int ToISeriesDateInt(this DateOnly date)
        => date.Year * 10000 + date.Month * 100 + date.Day;

    internal static string ToISeriesDateString(this DateOnly date)
        => date.ToString("yyyy-MM-dd");

    internal static char ToCharYN(this bool value) => value ? 'Y' : 'N';

    internal static int ToInt01(this bool value) => value ? 1 : 0;

    internal static TaxCalcWireRequest ToWire(this TaxCalculationRequest r) => new()
    {
        MasterDealerNumber = MasterDealerNumber, // C-5: must be sent on every tax calc call
        LotNumber = r.HomeCenterNumber,
        AppId = r.AppId,
        StockNumber = r.StockNumber,
        DomicileCode = $"{r.HomeCondition.ToChar()}{(r.NumberOfFloorSections <= 1 ? 'S' : 'D')}",
        ModCode = r.ModularClassification.ToModCodeChar(),
        Hbpp = r.WarrantyAmount
    };

    internal static TaxCalculationResult ToDomain(this TaxCalcWireResponse r) => new()
    {
        StateTax = r.StateTax,
        CityTax = r.CityTax,
        CountyTax = r.CountyTax,
        Basis = r.Basis,
        UseTax = r.UseTax,
        GrossReceiptCityTax = r.GrossReceiptCityTax,
        GrossReceiptCountyTax = r.GrossReceiptCountyTax,
        ManufacturedHomeInventoryTax = r.ManufacturedHomeInventoryTax,
        Messages = r.Messages
    };

    internal static AllowanceWireRequest ToWire(this AllowanceUpdateRequest r) => new()
    {
        MasterDealerNumber = MasterDealerNumber,
        AppId = r.AppId,
        CorrelationId = r.CorrelationId,
        LotNumber = r.HomeCenterNumber,
        HomeSalePrice = r.HomeSalePrice,
        HomeNetInvoice = r.HomeNetInvoice,
        NumberOfFloors = r.NumberOfFloorSections,
        FreightCost = r.FreightCost,
        CarrierFrameDeposit = r.CarrierFrameDeposit,
        HomeGrossInvoiceCost = r.GrossCost,
        HomeTaxOnInvoice = r.TaxIncludedOnInvoice,
        ManufacturerRebate = r.RebateOnMfgInvoice,
        TransportType = r.HomeCondition.ToChar(),
        TradeInAllowance = r.TradeAllowance,
        TradeInOverAllowance = r.BookInAmount,
        TradeInType = r.TradeInType ?? ' ',
        PreviouslyTitledInState = r.PreviouslyTitled,
        IsTaxExempt = r.IsTaxExempt,
        PointOfDeliveryCity = r.City,
        PointOfDeliveryCounty = r.County,
        PointOfDeliveryZipCode = r.PostalCode,
        IsPointOfDeliveryInCityLimits = r.IsWithinCityLimits,
        PointOfSaleZipCode = r.PointOfSaleZip,
        TotalAddOnCost = r.TotalAddOnCost,
        TotalAddOnSalePrice = r.TotalAddOnSalePrice,
        AddOnOptions = r.AddOns.Select(a => new AllowanceAddOnWire
        {
            CategoryId = a.CategoryNumber,
            ItemId = a.ItemNumber,
            Cost = a.Cost,
            SalePrice = a.SalePrice
        }).ToArray()
    };

    internal static TaxQuestionWireDto[] ToWire(this InsertTaxQuestionAnswersRequest r)
        => r.Answers.Select(a => new TaxQuestionWireDto
        {
            AppId = a.AppId,
            CustomerNumber = a.CustomerNumber,
            QuestionNumber = a.QuestionNumber,
            QuestionAnswer = a.Answer
        }).ToArray();

    internal static HomeFirstWireRequest ToWire(this HomeFirstQuoteRequest r) => new()
    {
        LotNumber = r.HomeCenterNumber,
        HomeStockNumber = r.StockNumber,
        HomeModel = r.ModelNumber,
        CoverageAmount = r.CoverageAmount,
        HomeYearBuilt = r.ModelYear,
        HomeType = r.HomeCondition.ToChar().ToString(),
        HomeSerial = r.SerialNumber,
        HomeLength = r.LengthInFeet,
        HomeWidth = r.WidthInFeet,
        OccupancyType = r.OccupancyType.ToChar(),
        IsHomeLocatedInPark = r.InParkOrSubdivision,
        IsHomeOnPermanentFoundation = r.HasFoundationOrMasonry,
        IsLandCustomerOwned = r.IsLandOwnedByCustomer,
        IsInCityLimits = r.IsWithinCityLimits,
        CustomerFirstName = r.FirstName,
        CustomerLastName = r.LastName,
        MailingAddress = r.MailingAddress,
        MailingCity = r.MailingCity,
        MailingState = r.MailingState,
        MailingZip = r.MailingZip,
        PhysicalAddress = r.LocationAddress,
        PhysicalCity = r.LocationCity,
        PhysicalState = r.LocationState,
        PhysicalZip = r.DeliveryZipCode,
        CustomerBirthDate = r.BuyerBirthDate?.ToDateTime(TimeOnly.MinValue),
        CoApplicantBirthDate = r.CoBuyerBirthDate?.ToDateTime(TimeOnly.MinValue),
        HomePhone = r.PhoneNumber
    };

    internal static HomeFirstQuoteResult ToDomain(this HomeFirstWireResponse r) => new()
    {
        InsuranceCompanyName = r.InsuranceCompanyName,
        TotalPremium = r.TotalPremium,
        MaximumCoverage = r.MaximumCoverage,
        TempLinkId = r.TempLinkId,
        ErrorMessage = r.ErrorMessage
    };

    internal static WarrantyWireRequest ToWire(this WarrantyQuoteRequest r) => new()
    {
        MasterDealerNumber = MasterDealerNumber,
        HomeCenterNumber = r.HomeCenterNumber,
        AppId = r.AppId,
        PhysicalState = r.PhysicalState,
        PhysicalZip = r.PhysicalZip,
        WidthInFeet = r.WidthInFeet,
        ModelYear = r.ModelYear,
        HomeType = r.HomeCondition.ToChar().ToString(),
        HudOrMod = r.ModularClassification.ToHudOrModString(),
        IsInCityLimits = r.IsWithinCityLimits,
        CalculateSalesTax = r.CalculateSalesTax
    };

    internal static WarrantyQuoteResult ToDomain(this WarrantyWireResponse r) => new()
    {
        Premium = r.Premium,
        SalesTaxPremium = r.SalesTaxPremium
    };

    internal static CommissionWireRequest ToWire(this CommissionCalculationRequest r) => new()
    {
        LinkId = r.AppId,
        Cost = r.Cost,
        LandPayoff = r.LandPayoff,
        LandImprovements = r.LandImprovements,
        AdjustedCost = r.AdjustedCost,
        PEMPL = r.EmployeeNumber,
        HOMETYPE = r.HomeCondition.ToChar().ToString(),
        MHC = r.HomeCenterNumber,
        Splits = r.Splits.Select(s => new CommissionSplitWire
        {
            EmployeeNumber = s.EmployeeNumber,
            Pay = s.PayPercentage,
            Gpp = s.GrossPayPercentage,
            TotalCommissionRate = s.TotalCommissionRate
        }).ToArray()
    };

    internal static CommissionResult ToDomain(this CommissionWireResponse r) => new()
    {
        CommissionableGrossProfit = r.CommissionableGrossProfit,
        TotalCommission = r.TotalCommission,
        EmployeeSplits = r.EmployeeSplits.Select(s => new CommissionSplitResult
        {
            EmployeeNumber = s.EmployeeNumber,
            Pay = s.Pay,
            GrossPayPercentage = s.GrossPayPercentage,
            TotalCommissionRate = s.TotalCommissionRate
        }).ToArray()
    };

    internal static OptionTotalsWireRequest ToWire(this OptionTotalsRequest r) => new()
    {
        HomeCenterState = r.HomeCenterState,
        EffectiveDate = r.EffectiveDate.ToISeriesDateString(),
        PlantNumber = r.PlantNumber,
        QuoteNumber = r.QuoteNumber,
        OrderNumber = r.OrderNumber
    };

    internal static OptionTotalsResult ToDomain(this OptionTotalsWireResponse r) => new()
    {
        FactoryOptionTotal = r.HbgOptionTotal,
        RetailOptionTotal = r.RetailOptionTotal
    };

    internal static RetailPriceWireRequest ToWire(this RetailPriceRequest r) => new()
    {
        HomeCenterState = r.HomeCenterState,
        EffectiveDate = r.EffectiveDate.ToISeriesDateString(),
        SerialNumber = r.SerialNumber,
        InvoiceTotalAmount = r.InvoiceTotalAmount,
        NumberOfAxles = r.NumberOfAxles,
        HbgOptionTotal = r.FactoryOptionTotal,
        RetailOptionTotal = r.RetailOptionTotal,
        ModelNumber = r.ModelNumber,
        BaseCost = r.BaseCost
    };

    /// <summary>
    /// Maps a trade-in type string to the single-char code expected by the iSeries.
    /// Legacy uses a dedicated lookup; the naive tt[0] approach gives wrong codes
    /// for "Modular Home" ('M' instead of 'D') and "Motorcycle" ('M' instead of 'C').
    /// </summary>
    internal static char? MapTradeInTypeCode(string? tradeType) => tradeType switch
    {
        "Single Wide" => 'S',
        "Double Wide" => 'D',
        "Modular Home" => 'D',
        "Motorcycle" => 'C',
        "Boat" => 'B',
        "Motor Vehicle" => 'V',
        "Travel Trailer" => 'T',
        "5th Wheel" or "Fifth Wheel" => 'F',
        _ when tradeType is { Length: > 0 } => tradeType[0],
        _ => null
    };
}
