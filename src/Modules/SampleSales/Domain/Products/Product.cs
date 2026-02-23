using Modules.SampleSales.Domain.Products.Events;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.Results;
using Rtl.Core.Domain.ValueObjects;

namespace Modules.SampleSales.Domain.Products;

public sealed class Product : SoftDeletableEntity, IAggregateRoot
{
    private const int MaxNameLength = 200;

    private Product() {}

    public string Name { get; private set; } = string.Empty;

    [SensitiveData]
    public string? Description { get; private set; }

    [SensitiveData]
    public decimal? InternalCost { get; private set; }

    public Money Price { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public static Result<Product> Create(string name, string? description, decimal price, decimal? internalCost, string currency = Money.DefaultCurrency)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Product>(ProductErrors.NameEmpty);
        }

        if (name.Length > MaxNameLength)
        {
            return Result.Failure<Product>(ProductErrors.NameTooLong);
        }

        var priceResult = Money.Create(price, currency);
        if (priceResult.IsFailure)
        {
            return Result.Failure<Product>(ProductErrors.PriceInvalid);
        }

        if (price <= 0)
        {
            return Result.Failure<Product>(ProductErrors.PriceInvalid);
        }

        var product = new Product
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            InternalCost = internalCost,
            Price = priceResult.Value,
            IsActive = true
        };

        product.Raise(new ProductCreatedDomainEvent());

        return product;
    }

    public Result Update(string name, string? description, decimal price, bool isActive, string? currency = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ProductErrors.NameEmpty);
        }

        if (name.Length > MaxNameLength)
        {
            return Result.Failure(ProductErrors.NameTooLong);
        }

        var priceResult = Money.Create(price, currency ?? Price.Currency);
        if (priceResult.IsFailure)
        {
            return Result.Failure(ProductErrors.PriceInvalid);
        }

        if (price <= 0)
        {
            return Result.Failure(ProductErrors.PriceInvalid);
        }

        Name = name.Trim();
        Description = description?.Trim();
        Price = priceResult.Value;
        IsActive = isActive;

        Raise(new ProductUpdatedDomainEvent());

        return Result.Success();
    }
}
