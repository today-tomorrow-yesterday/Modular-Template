using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.Customers.Errors;
using Modules.Customer.Domain.Customers.Events;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Domain.Customers.Entities;

// Customer aggregate root — single concrete class replacing the Party → Person/Organization TPH hierarchy.
// Customers are always people today; Organization support removed as premature abstraction.
public sealed class Customer : Entity, IAggregateRoot
{
    private readonly List<ContactPoint> _contactPoints = [];
    private readonly List<CustomerIdentifier> _identifiers = [];
    private readonly List<SalesAssignment> _salesAssignments = [];

    private Customer() {}

    public Guid PublicId { get; private set; }
    public LifecycleStage LifecycleStage { get; private set; }

    public int HomeCenterNumber { get; private set; }
    public string? SalesforceUrl { get; private set; }

    public MailingAddress? MailingAddress { get; private set; }

    public IReadOnlyCollection<ContactPoint> ContactPoints => _contactPoints.AsReadOnly();
    public IReadOnlyCollection<CustomerIdentifier> Identifiers => _identifiers.AsReadOnly();

    public DateTimeOffset? SourceCreatedOn { get; private set; }
    public DateTimeOffset? SourceLastModifiedOn { get; private set; }
    public DateTime LastSyncedAtUtc { get; private set; }

    public CustomerName? Name { get; private set; }
    [SensitiveData] public DateOnly? DateOfBirth { get; private set; }

    public IReadOnlyCollection<SalesAssignment> SalesAssignments => _salesAssignments.AsReadOnly();

    public int? CoBuyerCustomerId { get; private set; }
    public Customer? CoBuyer { get; private set; }

    // ─── Factory Methods ───────────────────────────────────────────

    public static Customer CreateLead(
        int homeCenterNumber,
        CustomerName? name,
        string salesforceLeadId,
        string? salesforceUrl)
    {
        var customer = new Customer
        {
            PublicId = Guid.CreateVersion7(),
            LifecycleStage = LifecycleStage.Lead,
            Name = name,
            HomeCenterNumber = homeCenterNumber,
            SalesforceUrl = salesforceUrl,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        customer.AddIdentifierInternal(IdentifierType.SalesforceLeadId, salesforceLeadId);
        customer.Raise(new CustomerCreatedDomainEvent());

        return customer;
    }

    public static Customer SyncFromCrm(
        int crmPartyId,
        int homeCenterNumber,
        LifecycleStage lifecycleStage,
        CustomerName? name,
        DateOnly? dateOfBirth,
        (string SalesPersonId, SalesAssignmentRole Role)[] salesAssignments,
        string? salesforceUrl,
        MailingAddress? mailingAddress,
        DateTimeOffset? createdOn,
        DateTimeOffset? lastModifiedOn)
    {
        var customer = new Customer
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

        customer.AddIdentifierInternal(IdentifierType.CrmPartyId, crmPartyId.ToString());

        foreach (var (salesPersonId, role) in salesAssignments)
        {
            customer._salesAssignments.Add(
                SalesAssignment.Create(customer.Id, salesPersonId, role));
        }

        customer.Raise(new CustomerCreatedDomainEvent());

        return customer;
    }

    public static Customer OnboardFromLoan(
        string loanId,
        int homeCenterNumber,
        CustomerName name,
        DateOnly? dateOfBirth,
        string? email,
        string? mobilePhone)
    {
        var customer = new Customer
        {
            PublicId = Guid.CreateVersion7(),
            LifecycleStage = LifecycleStage.Customer,
            Name = name,
            DateOfBirth = dateOfBirth,
            HomeCenterNumber = homeCenterNumber,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        customer.AddIdentifierInternal(IdentifierType.LoanId, loanId);

        if (!string.IsNullOrWhiteSpace(email))
        {
            customer.AddContactPointInternal(ContactPointType.Email, email, isPrimary: true);
        }

        if (!string.IsNullOrWhiteSpace(mobilePhone))
        {
            customer.AddContactPointInternal(ContactPointType.MobilePhone, mobilePhone, isPrimary: true);
        }

        customer.Raise(new CustomerOnboardedFromLoanDomainEvent());

        return customer;
    }

    // ─── Lifecycle Transitions ─────────────────────────────────────

    public Result PromoteToOpportunity(string salesforceOpportunityId)
    {
        if (LifecycleStage != LifecycleStage.Lead)
        {
            return Result.Failure(CustomerErrors.InvalidLifecycleTransition);
        }

        LifecycleStage = LifecycleStage.Opportunity;
        AddIdentifierInternal(IdentifierType.SalesforceOpportunityId, salesforceOpportunityId);
        LastSyncedAtUtc = DateTime.UtcNow;

        Raise(new CustomerLifecycleAdvancedDomainEvent(LifecycleStage.Opportunity));

        return Result.Success();
    }

    public Result PromoteToCustomer(string salesforceAccountId)
    {
        if (LifecycleStage != LifecycleStage.Opportunity)
        {
            return Result.Failure(CustomerErrors.InvalidLifecycleTransition);
        }

        LifecycleStage = LifecycleStage.Customer;
        AddIdentifierInternal(IdentifierType.SalesforceAccountId, salesforceAccountId);
        LastSyncedAtUtc = DateTime.UtcNow;

        Raise(new CustomerLifecycleAdvancedDomainEvent(LifecycleStage.Customer));

        return Result.Success();
    }

    // ─── Behavioral Methods ────────────────────────────────────────

    // Sales assignments use replace-all semantics (CDC is full-state, not delta).
    public void UpdateFromCrmSync(
        CustomerName? name,
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

        var homeCenterChanged = HomeCenterNumber != homeCenterNumber;
        var mailingAddressChanged = !Equals(MailingAddress, mailingAddress);

        HomeCenterNumber = homeCenterNumber;
        SalesforceUrl = salesforceUrl;
        MailingAddress = mailingAddress;
        SourceLastModifiedOn = lastModifiedOn;
        LastSyncedAtUtc = DateTime.UtcNow;

        if (homeCenterChanged)
        {
            Raise(new CustomerHomeCenterChangedDomainEvent(homeCenterNumber));
        }

        if (mailingAddressChanged)
        {
            Raise(new CustomerMailingAddressChangedDomainEvent());
        }

        if (nameChanged)
        {
            Raise(new CustomerNameChangedDomainEvent());
        }

        if (!previousAssignments.SequenceEqual(currentAssignments))
        {
            Raise(new CustomerSalesAssignmentsChangedDomainEvent());
        }
    }

    public void AddContactPoint(ContactPointType type, string value, bool isPrimary = false)
    {
        if (isPrimary)
        {
            foreach (var existing in _contactPoints.Where(cp => cp.Type == type && cp.IsPrimary))
            {
                existing.SetPrimary(false);
            }
        }

        // Upsert by Type + Value (natural key) — prevents duplicate rows on repeated syncs
        var match = _contactPoints.FirstOrDefault(cp => cp.Type == type && cp.Value == value);
        if (match is not null)
        {
            match.SetPrimary(isPrimary);
            return;
        }

        AddContactPointInternal(type, value, isPrimary);
    }

    public void ReplaceContactPoints(IEnumerable<(ContactPointType Type, string Value, bool IsPrimary)> incoming)
    {
        var incomingList = incoming.ToList();

        var previous = _contactPoints
            .Select(cp => (cp.Type, cp.Value, cp.IsPrimary))
            .OrderBy(x => x.Type).ThenBy(x => x.Value)
            .ToList();

        var incomingSet = incomingList
            .Select(cp => (cp.Type, cp.Value))
            .ToHashSet();

        var toRemove = _contactPoints
            .Where(cp => !incomingSet.Contains((cp.Type, cp.Value)))
            .ToList();
        foreach (var cp in toRemove)
        {
            _contactPoints.Remove(cp);
        }

        foreach (var (type, value, isPrimary) in incomingList)
        {
            var match = _contactPoints.FirstOrDefault(cp => cp.Type == type && cp.Value == value);
            if (match is not null)
            {
                match.SetPrimary(isPrimary);
            }
            else
            {
                AddContactPointInternal(type, value, isPrimary);
            }
        }

        var current = _contactPoints
            .Select(cp => (cp.Type, cp.Value, cp.IsPrimary))
            .OrderBy(x => x.Type).ThenBy(x => x.Value)
            .ToList();

        if (!previous.SequenceEqual(current))
        {
            Raise(new CustomerContactPointsChangedDomainEvent());
        }
    }

    public void AddIdentifier(IdentifierType type, string value) => AddIdentifierInternal(type, value);

    public string? GetIdentifierValue(IdentifierType type) => _identifiers.FirstOrDefault(i => i.Type == type)?.Value;

    public void UpdateMailingAddress(MailingAddress? address) => MailingAddress = address;

    public void AssignSalesPerson(string salesPersonId, SalesAssignmentRole role)
    {
        var existing = _salesAssignments.FirstOrDefault(sa => sa.SalesPersonId == salesPersonId);
        if (existing is not null)
        {
            if (existing.Role == role) return;
            _salesAssignments.Remove(existing);
        }

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

    public void SetCoBuyer(int coBuyerCustomerId)
    {
        if (CoBuyerCustomerId == coBuyerCustomerId) return;
        CoBuyerCustomerId = coBuyerCustomerId;
        Raise(new CustomerCoBuyerChangedDomainEvent());
    }

    public void RemoveCoBuyer()
    {
        if (CoBuyerCustomerId is null) return;
        CoBuyerCustomerId = null;
        Raise(new CustomerCoBuyerChangedDomainEvent());
    }

    // ─── Private Helpers ──────────────────────────────────────────

    private void AddContactPointInternal(ContactPointType type, string value, bool isPrimary) =>
        _contactPoints.Add(ContactPoint.Create(Id, type, value, isPrimary));

    private void AddIdentifierInternal(IdentifierType type, string value)
    {
        var existing = _identifiers.FirstOrDefault(i => i.Type == type);
        if (existing is not null)
        {
            existing.UpdateValue(value);
        }
        else
        {
            _identifiers.Add(CustomerIdentifier.Create(Id, type, value));
        }
    }
}
