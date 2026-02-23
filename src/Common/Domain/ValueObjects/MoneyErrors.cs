using Rtl.Core.Domain.Results;

namespace Rtl.Core.Domain.ValueObjects;

public static class MoneyErrors
{
    public static readonly Error NegativeAmount =
        Error.Validation("Money.NegativeAmount", "Money amount cannot be negative.");

    public static readonly Error InvalidCurrency =
        Error.Validation("Money.InvalidCurrency", "Currency code cannot be empty.");

    public static readonly Error InvalidCurrencyFormat =
        Error.Validation("Money.InvalidCurrencyFormat", "Currency code must be a 3-letter ISO code.");

    public static readonly Error CurrencyMismatch =
        Error.Validation("Money.CurrencyMismatch", "Cannot perform operations on money with different currencies.");
}
