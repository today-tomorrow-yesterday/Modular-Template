using Rtl.Core.Domain.Results;

namespace Rtl.Core.Domain.ValueObjects;

/// <summary>
/// Value object representing monetary amounts with currency.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public const string DefaultCurrency = "USD";

    // Private parameterless constructor for EF Core
    private Money()
    {
        Amount = 0;
        Currency = DefaultCurrency;
    }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = DefaultCurrency;

    /// <summary>
    /// Creates a new Money value object with validation.
    /// </summary>
    public static Result<Money> Create(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
        {
            return Result.Failure<Money>(MoneyErrors.NegativeAmount);
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            return Result.Failure<Money>(MoneyErrors.InvalidCurrency);
        }

        currency = currency.ToUpperInvariant();

        if (currency.Length != 3)
        {
            return Result.Failure<Money>(MoneyErrors.InvalidCurrencyFormat);
        }

        return new Money(amount, currency);
    }

    /// <summary>
    /// Internal factory for EF Core materialization - bypasses validation.
    /// </summary>
    internal static Money FromDatabase(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Creates a zero Money value.
    /// </summary>
    public static Money Zero(string currency = DefaultCurrency)
    {
        return new Money(0, currency.ToUpperInvariant());
    }

    /// <summary>
    /// Adds two Money values. Currencies must match.
    /// </summary>
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyMismatch);
        }

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies Money by a quantity.
    /// </summary>
    public Money Multiply(int quantity)
    {
        return new Money(Amount * quantity, Currency);
    }

    /// <summary>
    /// Multiplies Money by a decimal factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public bool Equals(Money? other)
    {
        if (other is null)
        {
            return false;
        }

        return Amount == other.Amount && Currency == other.Currency;
    }

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public override string ToString() => $"{Amount:F2} {Currency}";
}
