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
        int crmPartyId,
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

        person.AddIdentifierInternal(IdentifierType.CrmPartyId, crmPartyId.ToString());

        foreach (var (salesPersonId, role) in salesAssignments)
        {
            person._salesAssignments.Add(
                SalesAssignment.Create(person.Id, salesPersonId, role));
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

        var currentAssignments = _salesAssignments
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
        // Check if already assigned — if same role, no-op; if different role, remove to re-add
        var existing = _salesAssignments.FirstOrDefault(sa => sa.SalesPersonId == salesPersonId);
        if (existing is not null)
        {
            if (existing.Role == role) return;
            _salesAssignments.Remove(existing);
        }

        // Primary: only one allowed — demote current primary
        if (role == SalesAssignmentRole.Primary)
        {
            var currentPrimary = _salesAssignments.FirstOrDefault(sa => sa.Role == SalesAssignmentRole.Primary);
            if (currentPrimary is not null)
            {
                _salesAssignments.Remove(currentPrimary);
            }
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
