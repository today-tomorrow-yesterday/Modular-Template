using Modules.SampleSales.Domain.Catalogs.Events;
using ModularTemplate.Domain.Entities;
using ModularTemplate.Domain.Results;
using ModularTemplate.Domain.ValueObjects;

namespace Modules.SampleSales.Domain.Catalogs;

public sealed class Catalog : SoftDeletableEntity, IAggregateRoot
{
    private const int MaxNameLength = 200;
    private readonly List<CatalogProduct> _products = [];

    private Catalog() {}

    public Guid PublicId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public IReadOnlyCollection<CatalogProduct> Products => _products.AsReadOnly();

    public static Result<Catalog> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Catalog>(CatalogErrors.NameEmpty);
        }

        if (name.Length > MaxNameLength)
        {
            return Result.Failure<Catalog>(CatalogErrors.NameTooLong);
        }

        var catalog = new Catalog
        {
            PublicId = Guid.CreateVersion7(),
            Name = name.Trim(),
            Description = description?.Trim()
        };

        catalog.Raise(new CatalogCreatedDomainEvent());

        return catalog;
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(CatalogErrors.NameEmpty);
        }

        if (name.Length > MaxNameLength)
        {
            return Result.Failure(CatalogErrors.NameTooLong);
        }

        Name = name.Trim();
        Description = description?.Trim();

        Raise(new CatalogUpdatedDomainEvent());

        return Result.Success();
    }

    public Result AddProduct(int productId, Guid publicProductId, Money? customPrice = null)
    {
        if (_products.Any(p => p.ProductId == productId))
        {
            return Result.Failure(CatalogErrors.ProductAlreadyInCatalog);
        }

        var catalogProduct = CatalogProduct.Create(Id, productId, customPrice);
        _products.Add(catalogProduct);

        Raise(new CatalogProductAddedDomainEvent(publicProductId));

        return Result.Success();
    }

    public Result RemoveProduct(int productId, Guid publicProductId)
    {
        var catalogProduct = _products.FirstOrDefault(p => p.ProductId == productId);
        if (catalogProduct is null)
        {
            return Result.Failure(CatalogErrors.ProductNotInCatalog);
        }

        _products.Remove(catalogProduct);

        Raise(new CatalogProductRemovedDomainEvent(publicProductId));

        return Result.Success();
    }
}
