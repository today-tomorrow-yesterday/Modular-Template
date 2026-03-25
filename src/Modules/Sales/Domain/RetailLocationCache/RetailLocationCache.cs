using Modules.Sales.Domain.Sales;
using Rtl.Core.Domain.Caching;
using System.Text.Json;

namespace Modules.Sales.Domain.RetailLocationCache;

// Cache entity — cache.retail_location_cache. Sole Organization data target in Sales module.
// Populated by HomeCenterChanged integration event.
// HomeCenterNumber is NOT a first-class API parameter — CreateSaleCommand is the sole entry point.
// All other handlers derive via sale.RetailLocation.RefHomeCenterNumber.
public sealed class RetailLocationCache : ICacheProjection
{
    private readonly List<Sale> _sales = [];

    private RetailLocationCache() { }

    public int Id { get; set; }
    public RetailLocationType LocationType { get; private set; } // HomeCenter or Hub
    public string Name { get; private set; } = string.Empty; // LotName from Organization
    public string StateCode { get; private set; } = string.Empty; // Two-letter state code — tax jurisdiction
    public string Zip { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } // Only active locations can create new sales
    public int? RefHomeCenterNumber { get; private set; }
    public JsonDocument? OrganizationMetadata { get; private set; }
    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation
    public IReadOnlyCollection<Sale> Sales => _sales.AsReadOnly();

    public static RetailLocationCache CreateHomeCenter(
        int homeCenterNumber,
        string name,
        string stateCode,
        string zip,
        bool isActive,
        JsonDocument? organizationMetadata = null)
    {
        return new RetailLocationCache
        {
            LocationType = RetailLocationType.HomeCenter,
            RefHomeCenterNumber = homeCenterNumber,
            Name = name,
            StateCode = stateCode,
            Zip = zip,
            IsActive = isActive,
            OrganizationMetadata = organizationMetadata,
            LastSyncedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateFromHomeCenterChanged(
        string name,
        string stateCode,
        string zip,
        bool isActive,
        JsonDocument? organizationMetadata)
    {
        Name = name;
        StateCode = stateCode;
        Zip = zip;
        IsActive = isActive;
        OrganizationMetadata = organizationMetadata;
        LastSyncedAtUtc = DateTime.UtcNow;
    }
}
