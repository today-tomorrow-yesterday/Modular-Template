using Modules.Customer.Domain;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.Customers.Errors;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;
using System.Text.RegularExpressions;

namespace Modules.Customer.Application.Customers.OnboardCustomerFromLoan;

internal sealed partial class OnboardCustomerFromLoanCommandHandler(
    ICustomerRepository customerRepository,
    IVmfLosAdapter vmfLosAdapter,
    IUnitOfWork<ICustomerModule> unitOfWork)
    : ICommandHandler<OnboardCustomerFromLoanCommand, Guid>
{
    private const string CoBuyerLoanIdSuffix = "-cobuyer";

    public async Task<Result<Guid>> Handle(
        OnboardCustomerFromLoanCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await customerRepository.GetByIdentifierAsync(
            IdentifierType.LoanId,
            request.LoanId,
            cancellationToken);

        if (existing is not null)
        {
            return existing.PublicId;
        }

        var vmfResponse = await vmfLosAdapter.GetBorrowerByLoanIdAsync(
            request.LoanId,
            cancellationToken);

        var noBorrowersReturned = vmfResponse is null || vmfResponse.Borrowers.Length == 0;
        if (noBorrowersReturned)
        {
            return Result.Failure<Guid>(CustomerErrors.BorrowerNotFound);
        }

        if (vmfResponse!.ProspectorId is > 0)
        {
            var existingByProspector = await customerRepository.GetForUpdateByIdentifierAsync(
                IdentifierType.CrmPartyId,
                vmfResponse.ProspectorId.Value.ToString(),
                cancellationToken);

            if (existingByProspector is not null)
            {
                existingByProspector.AddIdentifier(IdentifierType.LoanId, request.LoanId);
                customerRepository.Update(existingByProspector);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return existingByProspector.PublicId;
            }
        }

        var borrower = vmfResponse.Borrowers[0];

        var customer = Domain.Customers.Entities.Customer.OnboardFromLoan(
            request.LoanId,
            request.HomeCenterNumber,
            CustomerName.Create(
                borrower.FirstName,
                borrower.MiddleName,
                borrower.LastName,
                borrower.Suffix),
            borrower.DateOfBirth.HasValue
                ? DateOnly.FromDateTime(borrower.DateOfBirth.Value)
                : null,
            borrower.Email,
            FormatPhone(borrower.CellPhone));

        var homePhone = FormatPhone(borrower.HomePhone);
        if (!string.IsNullOrWhiteSpace(homePhone))
        {
            customer.AddContactPoint(ContactPointType.HomePhone, homePhone);
        }

        if (vmfResponse.Borrowers.Length > 1)
        {
            var coBorrower = vmfResponse.Borrowers[1];
            var coBuyer = Domain.Customers.Entities.Customer.OnboardFromLoan(
                request.LoanId + CoBuyerLoanIdSuffix,
                request.HomeCenterNumber,
                CustomerName.Create(coBorrower.FirstName, coBorrower.MiddleName, coBorrower.LastName, coBorrower.Suffix),
                coBorrower.DateOfBirth.HasValue
                    ? DateOnly.FromDateTime(coBorrower.DateOfBirth.Value)
                    : null,
                coBorrower.Email,
                FormatPhone(coBorrower.CellPhone));

            customerRepository.Add(coBuyer);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            customer.SetCoBuyer(coBuyer.Id);
        }

        customerRepository.Add(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.PublicId;
    }

    private static string? FormatPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var cleaned = PhoneCleanupRegex().Replace(phone, "").TrimStart('+');
        return cleaned.Length > 0 ? cleaned : null;
    }

    [GeneratedRegex(@"[^\d+]")]
    private static partial Regex PhoneCleanupRegex();
}
