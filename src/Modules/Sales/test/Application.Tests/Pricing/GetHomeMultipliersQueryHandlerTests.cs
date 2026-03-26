using Modules.Sales.Application.Pricing.GetHomeMultipliers;
using Modules.Sales.Domain.Cdc;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Pricing;

public sealed class GetHomeMultipliersQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly ICdcPricingQueries _cdcPricingQueries = Substitute.For<ICdcPricingQueries>();
    private readonly GetHomeMultipliersQueryHandler _sut;

    public GetHomeMultipliersQueryHandlerTests() =>
        _sut = new GetHomeMultipliersQueryHandler(_saleRepository, _cdcPricingQueries);

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        var publicId = Guid.NewGuid();
        _saleRepository.GetByPublicIdWithCustomerContextAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetHomeMultipliersQuery(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_multiplier_not_found()
    {
        var sale = CreateSaleWithRetailLocation("TX");
        _saleRepository.GetByPublicIdWithCustomerContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _cdcPricingQueries.GetActiveMultiplierForStateAsync("TX", Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns((CdcPricingHomeMultiplier?)null);

        var result = await _sut.Handle(
            new GetHomeMultipliersQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("HomeMultipliers.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_when_multiplier_found()
    {
        var sale = CreateSaleWithRetailLocation("OH");
        _saleRepository.GetByPublicIdWithCustomerContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);

        var multiplier = new CdcPricingHomeMultiplier
        {
            EffectiveDate = new DateOnly(2025, 1, 1),
            HomeMultiplierValue = 1.5m,
            UpgradesMultiplier = 1.2m,
            FreightMultiplier = 1.1m,
            WheelsAxlesMultiplier = 1.3m,
            DuesMultiplier = 1.05m
        };
        _cdcPricingQueries.GetActiveMultiplierForStateAsync("OH", Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns(multiplier);

        var result = await _sut.Handle(
            new GetHomeMultipliersQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.5m, result.Value.BaseHomeMultiplier);
    }

    private static Sale CreateSaleWithRetailLocation(string stateCode)
    {
        var sale = Sale.Create(customerId: 1, retailLocationId: 1, saleType: SaleType.B2C);
        sale.ClearDomainEvents();

        var retailLocation = RetailLocationCacheEntity.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: stateCode, zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        return sale;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(obj, value);
        }
    }
}
