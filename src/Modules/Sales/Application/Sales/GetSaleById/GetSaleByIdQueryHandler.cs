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
        // Step 1: Load sale with party (including Person TPT detail) and retail location
        var sale = await saleRepository.GetByPublicIdWithPartyContextAsync(
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

        var person = sale.Party.Person;
        var customer = new SaleCustomerResult(
            RefCustomerId: sale.Party.RefPartyId,
            FirstName: person?.FirstName ?? sale.Party.DisplayName,
            MiddleName: person?.MiddleName,
            LastName: person?.LastName ?? string.Empty,
            Email: person?.Email,
            Phone: person?.Phone,
            MobilePhone: null, // Not on cache yet
            HomePhone: null, // Not on cache yet
            Birthdate: null, // Not on cache yet
            HomeCenterNumber: sale.Party.HomeCenterNumber,
            SalesforceId: sale.Party.SalesforceAccountId,
            SalesforceUrl: null, // Not on cache yet
            CoBuyerFirstName: person?.CoBuyerFirstName,
            CoBuyerLastName: person?.CoBuyerLastName,
            CoBuyerBirthdate: null, // Not on cache yet
            MailingAddress: null, // Not on cache yet
            PrimarySalesPersonFederatedId: person?.PrimarySalesPersonFederatedId,
            PrimarySalesPersonFirstName: person?.PrimarySalesPersonFirstName,
            PrimarySalesPersonLastName: person?.PrimarySalesPersonLastName,
            SecondarySalesPersonFederatedId: person?.SecondarySalesPersonFederatedId,
            SecondarySalesPersonFirstName: person?.SecondarySalesPersonFirstName,
            SecondarySalesPersonLastName: person?.SecondarySalesPersonLastName);

        var result = new GetSaleByIdResult(
            sale.PublicId,
            sale.SaleNumber,
            sale.Party.RefPublicId,
            retailLocation,
            sale.SaleType.ToString(),
            sale.SaleStatus.ToString(),
            customer,
            sale.CreatedAtUtc);

        return result;
    }
}
