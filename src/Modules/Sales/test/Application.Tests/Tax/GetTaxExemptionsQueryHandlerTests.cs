using Modules.Sales.Application.Tax.GetTaxExemptions;
using Modules.Sales.Domain.Cdc;
using NSubstitute;
using Xunit;

namespace Modules.Sales.Application.Tests.Tax;

public sealed class GetTaxExemptionsQueryHandlerTests
{
    private readonly ICdcTaxQueries _cdcTaxQueries = Substitute.For<ICdcTaxQueries>();
    private readonly GetTaxExemptionsQueryHandler _sut;

    public GetTaxExemptionsQueryHandlerTests() =>
        _sut = new GetTaxExemptionsQueryHandler(_cdcTaxQueries);

    [Fact]
    public async Task Returns_success_with_empty_list_when_no_exemptions()
    {
        _cdcTaxQueries.GetActiveExemptionsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CdcTaxExemption>());

        var result = await _sut.Handle(
            new GetTaxExemptionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Returns_success_with_mapped_exemptions()
    {
        var exemption = new CdcTaxExemption
        {
            ExemptionCode = 1,
            Description = "Veteran Exemption",
            RulesText = "Must provide DD-214"
        };

        _cdcTaxQueries.GetActiveExemptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CdcTaxExemption> { exemption });

        var result = await _sut.Handle(
            new GetTaxExemptionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(1, result.Value.First().ExemptionCode);
        Assert.Equal("Veteran Exemption", result.Value.First().Description);
    }
}
