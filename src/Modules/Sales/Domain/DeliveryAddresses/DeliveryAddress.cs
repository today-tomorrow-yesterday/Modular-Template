using Modules.Sales.Domain.DeliveryAddresses.Events;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Sales.Domain.DeliveryAddresses;

public sealed class DeliveryAddress : AuditableEntity
{
    private static readonly string[] InsuranceIneligibleOccupancyTypes = ["Rental", "Investment"];

    private DeliveryAddress() { }

    public Guid PublicId { get; private set; }
    public int SaleId { get; private set; }

    public string? AddressStyle { get; private set; }
    public string? AddressType { get; private set; }
    [SensitiveData] public string? AddressLine1 { get; private set; }
    [SensitiveData] public string? AddressLine2 { get; private set; }
    [SensitiveData] public string? AddressLine3 { get; private set; }
    public string? City { get; private set; }
    public string? County { get; private set; }
    public string? State { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }

    public string? OccupancyType { get; private set; } // Determines insurance eligibility
    public bool IsWithinCityLimits { get; private set; } // Affects tax and warranty calculations

    public Sale Sale { get; private set; } = null!;

    public static DeliveryAddress Create(
        int saleId,
        string? occupancyType,
        bool isWithinCityLimits,
        string? addressStyle,
        string? addressType,
        string? addressLine1,
        string? addressLine2,
        string? addressLine3,
        string? city,
        string? county,
        string? state,
        string? country,
        string? postalCode)
    {
        var address = new DeliveryAddress
        {
            PublicId = Guid.CreateVersion7(),
            SaleId = saleId,
            OccupancyType = occupancyType,
            IsWithinCityLimits = isWithinCityLimits,
            AddressStyle = addressStyle,
            AddressType = addressType,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            AddressLine3 = addressLine3,
            City = city,
            County = county,
            State = state,
            Country = country,
            PostalCode = postalCode
        };

        address.Raise(new DeliveryAddressCreatedDomainEvent { SaleId = saleId, DeliveryAddressId = address.Id });

        return address;
    }

    public void Update(
        string? occupancyType,
        bool isWithinCityLimits,
        string? addressStyle,
        string? addressType,
        string? addressLine1,
        string? addressLine2,
        string? addressLine3,
        string? city,
        string? county,
        string? state,
        string? country,
        string? postalCode)
    {
        // Capture pre-update snapshot for change detection
        var oldState = State;
        var oldOccupancyType = OccupancyType;
        var oldCity = City;
        var oldPostalCode = PostalCode;
        var oldCounty = County;
        var oldIsWithinCityLimits = IsWithinCityLimits;

        OccupancyType = occupancyType;
        IsWithinCityLimits = isWithinCityLimits;
        AddressStyle = addressStyle;
        AddressType = addressType;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        AddressLine3 = addressLine3;
        City = city;
        County = county;
        State = state;
        Country = country;
        PostalCode = postalCode;

        Raise(new DeliveryAddressChangedDomainEvent { SaleId = SaleId, DeliveryAddressId = Id });

        if (StateChanged(oldState))
        {
            Raise(new DeliveryAddressStateChangedDomainEvent { SaleId = SaleId, OldState = oldState, NewState = State });
        }

        if (OccupancyBecameInsuranceIneligible(oldOccupancyType))
        {
            Raise(new DeliveryAddressOccupancyBecameIneligibleDomainEvent { SaleId = SaleId, NewOccupancyType = OccupancyType! });
        }

        if (LocationChanged(oldCity, oldState, oldPostalCode, oldCounty, oldIsWithinCityLimits))
        {
            Raise(new DeliveryAddressLocationChangedDomainEvent { SaleId = SaleId });
        }
    }

    private bool StateChanged(string? oldState) =>
        !string.Equals(oldState, State, StringComparison.OrdinalIgnoreCase);

    public static bool IsOccupancyInsuranceIneligible(string? occupancyType) =>
        occupancyType is not null &&
        InsuranceIneligibleOccupancyTypes.Contains(occupancyType, StringComparer.OrdinalIgnoreCase);

    private bool OccupancyBecameInsuranceIneligible(string? oldOccupancyType) =>
        IsOccupancyInsuranceIneligible(OccupancyType) &&
        !string.Equals(oldOccupancyType, OccupancyType, StringComparison.OrdinalIgnoreCase);

    private bool LocationChanged(
        string? oldCity, string? oldState, string? oldPostalCode,
        string? oldCounty, bool oldIsWithinCityLimits) =>
        !string.Equals(oldCity, City, StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(oldState, State, StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(oldPostalCode, PostalCode, StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(oldCounty, County, StringComparison.OrdinalIgnoreCase) ||
        oldIsWithinCityLimits != IsWithinCityLimits;
}
