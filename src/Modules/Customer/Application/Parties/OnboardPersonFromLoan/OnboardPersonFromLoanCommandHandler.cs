using Modules.Customer.Domain;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Errors;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;
using System.Text.RegularExpressions;

namespace Modules.Customer.Application.Parties.OnboardPersonFromLoan;

// Flow: Customer.OnboardPersonFromLoanCommand → upsert Customer.parties → raises Customer.PartyOnboardedFromLoanDomainEvent
internal sealed partial class OnboardPersonFromLoanCommandHandler(
    IPartyRepository partyRepository,
    IVmfLosAdapter vmfLosAdapter,
    IUnitOfWork<ICustomerModule> unitOfWork)
    : ICommandHandler<OnboardPersonFromLoanCommand, Guid>
{
    private const string CoBuyerLoanIdSuffix = "-cobuyer";

    public async Task<Result<Guid>> Handle(
        OnboardPersonFromLoanCommand request,
        CancellationToken cancellationToken)
    {
        // Dedup check — already onboarded?
        var existing = await partyRepository.GetByIdentifierAsync(
            IdentifierType.LoanId,
            request.LoanId,
            cancellationToken);

        if (existing is not null)
        {
            return existing.PublicId;
        }

        // Fetch borrower data from VMF LOS
        var vmfResponse = await vmfLosAdapter.GetBorrowerByLoanIdAsync(
            request.LoanId,
            cancellationToken);

        if (vmfResponse is null || vmfResponse.Borrowers.Length == 0)
        {
            return Result.Failure<Guid>(PartyErrors.BorrowerNotFound);
        }

        // ProspectorId dedup — already exists as a Party from CRM?
        if (vmfResponse.ProspectorId is > 0)
        {
            var existingByProspector = await partyRepository.GetByIdAsync(
                vmfResponse.ProspectorId.Value,
                cancellationToken);

            if (existingByProspector is Person existingPerson)
            {
                // Link the loan to existing Party via identifier
                existingPerson.AddIdentifier(IdentifierType.LoanId, request.LoanId);
                partyRepository.Update(existingPerson);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return existingPerson.PublicId;
            }
        }

        var borrower = vmfResponse.Borrowers[0];

        var person = Person.OnboardFromLoan(
            request.LoanId,
            request.HomeCenterNumber,
            PersonName.Create(
                borrower.FirstName,
                borrower.MiddleName,
                borrower.LastName,
                borrower.Suffix),
            borrower.DateOfBirth.HasValue
                ? DateOnly.FromDateTime(borrower.DateOfBirth.Value)
                : null,
            borrower.Email,
            FormatPhone(borrower.CellPhone));

        // Add home phone as secondary contact point
        if (!string.IsNullOrWhiteSpace(borrower.HomePhone))
        {
            person.AddContactPoint(ContactPointType.HomePhone, FormatPhone(borrower.HomePhone)!);
        }

        // CoBuyer as separate Person linked via FK
        if (vmfResponse.Borrowers.Length > 1)
        {
            var coBorrower = vmfResponse.Borrowers[1];
            var coBuyer = Person.OnboardFromLoan(
                request.LoanId + CoBuyerLoanIdSuffix,
                request.HomeCenterNumber,
                PersonName.Create(coBorrower.FirstName, coBorrower.MiddleName, coBorrower.LastName, coBorrower.Suffix),
                coBorrower.DateOfBirth.HasValue
                    ? DateOnly.FromDateTime(coBorrower.DateOfBirth.Value)
                    : null,
                coBorrower.Email,
                FormatPhone(coBorrower.CellPhone));

            partyRepository.Add(coBuyer);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            person.SetCoBuyer(coBuyer.Id);
        }

        partyRepository.Add(person);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return person.PublicId;
    }

    private static string? FormatPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        return PhoneCleanupRegex().Replace(phone, "");
    }

    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex PhoneCleanupRegex();
}
