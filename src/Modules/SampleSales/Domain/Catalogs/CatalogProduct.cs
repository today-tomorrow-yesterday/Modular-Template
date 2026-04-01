using ModularTemplate.Domain.Entities;
using ModularTemplate.Domain.ValueObjects;

namespace Modules.SampleSales.Domain.Catalogs;

/// <summary>
/// Entity representing a product within a Catalog aggregate.
/// Not an aggregate root - can only be accessed through the Catalog aggregate.
/// </summary>
public sealed class CatalogProduct : Entity
{
    private CatalogProduct() {}

    public int CatalogId { get; private set; }

    public int ProductId { get; private set; }

    public Money? CustomPrice { get; private set; }

    public DateTime AddedAtUtc { get; private set; }

    internal static CatalogProduct Create(int catalogId, int productId, Money? customPrice = null)
    {
        return new CatalogProduct
        {
            CatalogId = catalogId,
            ProductId = productId,
            CustomPrice = customPrice,
            AddedAtUtc = DateTime.UtcNow
        };
    }

    internal void UpdateCustomPrice(Money? customPrice)
    {
        CustomPrice = customPrice;
    }
}
