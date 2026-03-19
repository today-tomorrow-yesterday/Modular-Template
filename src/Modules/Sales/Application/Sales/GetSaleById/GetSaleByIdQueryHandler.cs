using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Sales.GetSaleById;

internal sealed class GetSaleByIdQueryHandler(ISaleRepository saleRepository)
    : IQueryHandler<GetSaleByIdQuery, GetSaleByIdResult>
{
    public async Task<Result<GetSaleByIdResult>> Handle(
        GetSaleByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale with customer and retail location
        var sale = await saleRepository.GetByPublicIdWithCustomerContextAsync(
            request.SalePublicId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<GetSaleByIdResult>(
                SaleErrors.NotFoundByPublicId(request.SalePublicId));
        }

        // Step 2: Map domain entities to response
        var retailLocation = new RetailLocationResult(
            sale.RetailLocation.Id,
            sale.RetailLocation.LocationType.ToString(),
            sale.RetailLocation.Name,
            sale.RetailLocation.StateCode,
            sale.RetailLocation.Zip,
            sale.RetailLocation.RefHomeCenterNumber);

        var customerCache = sale.Customer;
        var customer = new SaleCustomerResult(
            RefCustomerId: customerCache.RefPublicId,
            FirstName: customerCache.FirstName ?? customerCache.DisplayName,
            MiddleName: customerCache.MiddleName,
            LastName: customerCache.LastName ?? string.Empty,
            Email: customerCache.Email,
            Phone: customerCache.Phone,
            MobilePhone: null, // Not on cache yet
            HomePhone: null, // Not on cache yet
            Birthdate: null, // Not on cache yet
            HomeCenterNumber: customerCache.HomeCenterNumber,
            SalesforceId: customerCache.SalesforceAccountId,
            SalesforceUrl: null, // Not on cache yet
            CoBuyerFirstName: customerCache.CoBuyerFirstName,
            CoBuyerLastName: customerCache.CoBuyerLastName,
            CoBuyerBirthdate: null, // Not on cache yet
            MailingAddress: null, // Not on cache yet
            PrimarySalesPersonFederatedId: customerCache.PrimarySalesPersonFederatedId,
            PrimarySalesPersonFirstName: customerCache.PrimarySalesPersonFirstName,
            PrimarySalesPersonLastName: customerCache.PrimarySalesPersonLastName,
            SecondarySalesPersonFederatedId: customerCache.SecondarySalesPersonFederatedId,
            SecondarySalesPersonFirstName: customerCache.SecondarySalesPersonFirstName,
            SecondarySalesPersonLastName: customerCache.SecondarySalesPersonLastName);

        var result = new GetSaleByIdResult(
            sale.PublicId,
            sale.SaleNumber,
            sale.Customer.RefPublicId,
            retailLocation,
            sale.SaleType.ToString(),
            sale.SaleStatus.ToString(),
            customer,
            sale.CreatedAtUtc);

        return result;
    }
}
