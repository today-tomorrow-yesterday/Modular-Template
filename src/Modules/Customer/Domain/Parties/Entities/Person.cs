using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;
using Rtl.Core.Domain.Auditing;

namespace Modules.Customer.Domain.Parties.Entities;

// Person party — individual home buyer progressing through Lead -> Opportunity -> Customer.
// Person-specific: Name, DateOfBirth, SalesPersons, CoBuyer.
// ~95% of parties in SES Pro are Persons.
public sealed class Person : Party
{
    private readonly List<SalesAssignment> _salesAssignments = [];

    private Person() => PartyType = PartyType.Person;

    public PersonName? Name { get; private set; }
    [SensitiveData] public DateOnly? DateOfBirth { get; private set; } 

    public IReadOnlyCollection<SalesAssignment> SalesAssignments => _salesAssignments.AsReadOnly();

    public int? CoBuyerPartyId { get; private set; }
    public Party? CoBuyer { get; private set; }

    // ─── Factory Methods ───────────────────────────────────────────

    public static Person CreateLead(
        int homeCenterNumber,
        PersonName? name,
        string salesforceLeadId,
        string? salesforceUrl)
    {
        var person = new Person
        {
            PublicId = Guid.CreateVersion7(),
            LifecycleStage = LifecycleStage.Lead,
            Name = name,
            HomeCenterNumber = homeCenterNumber,
            SalesforceUrl = salesforceUrl,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        person.AddIdentifierInternal(IdentifierType.SalesforceLeadId, salesforceLeadId);
        person.Raise(new PartyCreatedDomainEvent());

        return person;
    }

    public static Person SyncFromCrm(
        int partyId,
        int homeCenterNumber,
        LifecycleStage lifecycleStage,
        PersonName? name,
        DateOnly? dateOfBirth,
        (string SalesPersonId, SalesAssignmentRole Role)[] salesAssignments,
        string? salesforceUrl,
        MailingAddress? mailingAddress,
        DateTimeOffset? createdOn,
        DateTimeOffset? lastModifiedOn)
    {
        var person = new Person
        {
            Id = partyId,
            PublicId = Guid.CreateVersion7(),
            LifecycleStage = lifecycleStage,
            Name = name,
            DateOfBirth = dateOfBirth,
            HomeCenterNumber = homeCenterNumber,
            SalesforceUrl = salesforceUrl,
            MailingAddress = mailingAddress,
            SourceCreatedOn = createdOn,
            SourceLastModifiedOn = lastModifiedOn,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        foreach (var (salesPersonId, role) in salesAssignments)
        {
            person._salesAssignments.Add(
                SalesAssignment.Create(partyId, salesPersonId, role));
        }

        person.Raise(new PartyCreatedDomainEvent());

        return person;
    }

    public static Person OnboardFromLoan(
        string loanId,
        int homeCenterNumber,
        PersonName name,
        DateOnly? dateOfBirth,
        string? email,
        string? mobilePhone)
    {
        var person = new Person
        {
            PublicId = Guid.CreateVersion7(),
            LifecycleStage = LifecycleStage.Customer,
            Name = name,
            DateOfBirth = dateOfBirth,
            HomeCenterNumber = homeCenterNumber,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        person.AddIdentifierInternal(IdentifierType.LoanId, loanId);

        if (!string.IsNullOrWhiteSpace(email))
        {
            person.AddContactPointInternal(ContactPointType.Email, email, isPrimary: true);
        }

        if (!string.IsNullOrWhiteSpace(mobilePhone))
        {
            person.AddContactPointInternal(ContactPointType.MobilePhone, mobilePhone, isPrimary: true);
        }

        person.Raise(new PartyOnboardedFromLoanDomainEvent());

        return person;
    }

    // ─── Behavioral Methods ────────────────────────────────────────

    // Sales assignments use replace-all semantics (CDC is full-state, not delta).
    public void UpdateFromCrmSync(
        PersonName? name,
        DateOnly? dateOfBirth,
        (string SalesPersonId, SalesAssignmentRole Role)[] salesAssignments,
        int homeCenterNumber,
        string? salesforceUrl,
        MailingAddress? mailingAddress,
        DateTimeOffset? lastModifiedOn)
    {
        var nameChanged = !Equals(Name, name);
        Name = name;
        DateOfBirth = dateOfBirth;

        // Replace-all: clear existing assignments and re-add from incoming CDC data
        var previousAssignments = _salesAssignments
            .Select(sa => (sa.SalesPersonId, sa.Role))
            .OrderBy(x => x.SalesPersonId)
            .ToList();

        _salesAssignments.Clear();
        foreach (var (salesPersonId, role) in salesAssignments)
        {
            _salesAssignments.Add(
                SalesAssignment.Create(Id, salesPersonId, role));
        }

        var currentAssignments = salesAssignments
            .Select(sa => (sa.SalesPersonId, sa.Role))
            .OrderBy(x => x.SalesPersonId)
            .ToList();

        ApplySharedCrmSyncFields(homeCenterNumber, salesforceUrl, mailingAddress, lastModifiedOn);

        if (nameChanged)
        {
            Raise(new PartyNameChangedDomainEvent());
        }

        if (!previousAssignments.SequenceEqual(currentAssignments))
        {
            Raise(new PartySalesAssignmentsChangedDomainEvent());
        }
    }

    public void AssignSalesPerson(string salesPersonId, SalesAssignmentRole role)
    {
        // Primary: replace existing (only one allowed)
        // Supporting: just add (multiple allowed, but not the same SalesPerson twice)
        if (role == SalesAssignmentRole.Primary)
        {
            var existingPrimary = _salesAssignments.FirstOrDefault(sa => sa.Role == SalesAssignmentRole.Primary);
            if (existingPrimary is not null)
            {
                _salesAssignments.Remove(existingPrimary);
            }
        }

        if (_salesAssignments.Any(sa => sa.SalesPersonId == salesPersonId))
        {
            return;
        }

        _salesAssignments.Add(SalesAssignment.Create(Id, salesPersonId, role));
    }

    public void RemoveSalesAssignment(string salesPersonId)
    {
        var existing = _salesAssignments.FirstOrDefault(sa => sa.SalesPersonId == salesPersonId);
        if (existing is not null)
        {
            _salesAssignments.Remove(existing);
        }
    }

    public string? GetPrimarySalesPersonId() =>
        _salesAssignments.FirstOrDefault(sa => sa.Role == SalesAssignmentRole.Primary)?.SalesPersonId;

    public void SetCoBuyer(int coBuyerPartyId)
    {
        if (CoBuyerPartyId == coBuyerPartyId) return;
        CoBuyerPartyId = coBuyerPartyId;
        Raise(new PartyCoBuyerChangedDomainEvent());
    }

    public void RemoveCoBuyer()
    {
        if (CoBuyerPartyId is null) return;
        CoBuyerPartyId = null;
        Raise(new PartyCoBuyerChangedDomainEvent());
    }
}
