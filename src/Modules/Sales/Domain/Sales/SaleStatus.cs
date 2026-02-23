namespace Modules.Sales.Domain.Sales;

public enum SaleStatus
{
    Unknown = 0,
    Inquiry = 10,
    Discovery = 20,
    Application = 30,
    Approval = 40,
    Processing = 50,
    Closing = 60,
    Construction = 70,
    Booked = 80
}
