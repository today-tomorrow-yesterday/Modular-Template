using Rtl.Core.Domain.ValueObjects;
using Xunit;

namespace Rtl.Core.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ReturnsSuccess()
    {
        var result = Money.Create(100.00m, "USD");

        Assert.True(result.IsSuccess);
        Assert.Equal(100.00m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ReturnsFailure()
    {
        var result = Money.Create(-10.00m);

        Assert.True(result.IsFailure);
        Assert.Equal(MoneyErrors.NegativeAmount, result.Error);
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var money1 = Money.Create(100.00m).Value;
        var money2 = Money.Create(50.00m).Value;

        var result = money1.Add(money2);

        Assert.True(result.IsSuccess);
        Assert.Equal(150.00m, result.Value.Amount);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ReturnsFailure()
    {
        var money1 = Money.Create(100.00m, "USD").Value;
        var money2 = Money.Create(50.00m, "EUR").Value;

        var result = money1.Add(money2);

        Assert.True(result.IsFailure);
        Assert.Equal(MoneyErrors.CurrencyMismatch, result.Error);
    }
}
