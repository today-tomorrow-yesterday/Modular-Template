using Modules.Sales.Application.ProjectCosts.GetProjectCostCategories;
using Modules.Sales.Domain.Cdc;
using NSubstitute;
using Xunit;

namespace Modules.Sales.Application.Tests.ProjectCosts;

public sealed class GetProjectCostCategoriesQueryHandlerTests
{
    private readonly ICdcProjectCostQueries _cdcProjectCostQueries = Substitute.For<ICdcProjectCostQueries>();
    private readonly GetProjectCostCategoriesQueryHandler _sut;

    public GetProjectCostCategoriesQueryHandlerTests() =>
        _sut = new GetProjectCostCategoriesQueryHandler(_cdcProjectCostQueries);

    [Fact]
    public async Task Returns_success_with_empty_collections_when_no_data()
    {
        _cdcProjectCostQueries.GetCategoriesWithItemsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CdcProjectCostCategory>());
        _cdcProjectCostQueries.GetStateMatrixAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CdcProjectCostStateMatrix>());

        var result = await _sut.Handle(
            new GetProjectCostCategoriesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Categories);
        Assert.Empty(result.Value.StateMatrix);
    }

    [Fact]
    public async Task Returns_success_with_mapped_categories_and_matrix()
    {
        var category = new CdcProjectCostCategory
        {
            CategoryNumber = 1,
            Description = "Setup",
            IsCreditConsideration = true,
            IsLandDot = false,
            RestrictFha = false,
            RestrictCss = false,
            DisplayForCash = true,
            Items = new List<CdcProjectCostItem>
            {
                new()
                {
                    ItemNumber = 10,
                    Description = "Skirting",
                    Status = "A",
                    IsFeeItem = false,
                    IsFhaRestricted = false,
                    IsCssRestricted = false,
                    IsDisplayForCash = true,
                    IsRestrictOptionPrice = false,
                    IsRestrictCssCost = false,
                    IsHopeRefundsIncluded = false,
                    ProfitPercentage = 10.5m
                }
            }
        };

        var matrix = new CdcProjectCostStateMatrix
        {
            CategoryId = 1,
            CategoryItemId = 10,
            HomeType = "MFG",
            StateCode = "OH",
            TaxBasisManufactured = 100m,
            TaxBasisModularOn = 80m,
            TaxBasisModularOff = 60m,
            IsInsurable = true
        };

        _cdcProjectCostQueries.GetCategoriesWithItemsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CdcProjectCostCategory> { category });
        _cdcProjectCostQueries.GetStateMatrixAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CdcProjectCostStateMatrix> { matrix });

        var result = await _sut.Handle(
            new GetProjectCostCategoriesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Categories);
        Assert.Single(result.Value.StateMatrix);
        Assert.Equal("Setup", result.Value.Categories.First().Description);
    }
}
