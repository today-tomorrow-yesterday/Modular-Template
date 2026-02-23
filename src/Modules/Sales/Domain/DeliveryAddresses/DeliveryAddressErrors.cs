using Rtl.Core.Domain.Results;

namespace Modules.Sales.Domain.DeliveryAddresses;

public static class DeliveryAddressErrors
{
    public static Error NotFound(int deliveryAddressId) =>
        Error.NotFound("DeliveryAddress.NotFound", $"Delivery address with ID '{deliveryAddressId}' was not found.");

    public static Error AlreadyExists(int saleId) =>
        Error.Conflict("DeliveryAddress.AlreadyExists", $"Sale '{saleId}' already has a delivery address.");
}
