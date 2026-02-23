using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Errors;
using Modules.Customer.Domain.Parties.Events;
using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Domain.Parties.Entities;

// Party aggregate root — abstract base for Person and Organization.
// Collapses enterprise 3-layer hierarchy (Retail Customer -> Customer Account -> Party)
// into single aggregate with LifecycleStage. EF Core TPH with "party_type" discriminator.
// Shared concerns live here. Type-specific fields on Person/Organization.
public abstract class Party : Entity, IAggregateRoot
{
    private readonly List<ContactPoint> _contactPoints = [];
    private readonly List<PartyIdentifier> _identifiers = [];

    protected Party() {}

    public Guid PublicId { get; protected set; }
    public PartyType PartyType { get; protected set; } 
    public LifecycleStage LifecycleStage { get; protected set; }

    public int HomeCenterNumber { get; protected set; }
    public string? SalesforceUrl { get; protected set; }

    public MailingAddress? MailingAddress { get; protected set; }

    public IReadOnlyCollection<ContactPoint> ContactPoints => _contactPoints.AsReadOnly();
    public IReadOnlyCollection<PartyIdentifier> Identifiers => _identifiers.AsReadOnly();

    public DateTimeOffset? SourceCreatedOn { get; protected set; }
    public DateTimeOffset? SourceLastModifiedOn { get; protected set; }
    public DateTime LastSyncedAtUtc { get; protected set; }

    // ─── Lifecycle Transitions (shared) ─────────────────────────────

    public Result PromoteToOpportunity(string salesforceOpportunityId)
    {
        if (LifecycleStage != LifecycleStage.Lead)
        {
            return Result.Failure(PartyErrors.InvalidLifecycleTransition);
        }

        LifecycleStage = LifecycleStage.Opportunity;
        AddIdentifierInternal(IdentifierType.SalesforceOpportunityId, salesforceOpportunityId);
        LastSyncedAtUtc = DateTime.UtcNow;

        Raise(new PartyLifecycleAdvancedDomainEvent(LifecycleStage.Opportunity));

        return Result.Success();
    }

    public Result PromoteToCustomer(string salesforceAccountId)
    {
        if (LifecycleStage != LifecycleStage.Opportunity)
        {
            return Result.Failure(PartyErrors.InvalidLifecycleTransition);
        }

        LifecycleStage = LifecycleStage.Customer;
        AddIdentifierInternal(IdentifierType.SalesforceAccountId, salesforceAccountId);
        LastSyncedAtUtc = DateTime.UtcNow;

        Raise(new PartyLifecycleAdvancedDomainEvent(LifecycleStage.Customer));

        return Result.Success();
    }

    // ─── Shared Behaviors ───────────────────────────────────────────

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
        var previous = _contactPoints
            .Select(cp => (cp.Type, cp.Value, cp.IsPrimary))
            .OrderBy(x => x.Type).ThenBy(x => x.Value)
            .ToList();

        _contactPoints.Clear();
        foreach (var (type, value, isPrimary) in incoming)
        {
            AddContactPointInternal(type, value, isPrimary);
        }

        var current = _contactPoints
            .Select(cp => (cp.Type, cp.Value, cp.IsPrimary))
            .OrderBy(x => x.Type).ThenBy(x => x.Value)
            .ToList();

        if (!previous.SequenceEqual(current))
        {
            Raise(new PartyContactPointsChangedDomainEvent());
        }
    }

    public void AddIdentifier(IdentifierType type, string value) => AddIdentifierInternal(type, value);

    public string? GetIdentifierValue(IdentifierType type) => _identifiers.FirstOrDefault(i => i.Type == type)?.Value;

    public void UpdateMailingAddress(MailingAddress? address) => MailingAddress = address;

    // ─── Protected Helpers (for derived class factory methods) ──────

    protected void AddContactPointInternal(ContactPointType type, string value, bool isPrimary) => _contactPoints.Add(ContactPoint.Create(Id, type, value, isPrimary));

    protected void AddIdentifierInternal(IdentifierType type, string value)
    {
        var existing = _identifiers.FirstOrDefault(i => i.Type == type);
        if (existing is not null)
        {
            existing.UpdateValue(value);
        }
        else
        {
            _identifiers.Add(PartyIdentifier.Create(Id, type, value));
        }
    }

    protected void ApplySharedCrmSyncFields(
        int homeCenterNumber,
        string? salesforceUrl,
        MailingAddress? mailingAddress,
        DateTimeOffset? lastModifiedOn)
    {
        var homeCenterChanged = HomeCenterNumber != homeCenterNumber;
        var mailingAddressChanged = !Equals(MailingAddress, mailingAddress);

        HomeCenterNumber = homeCenterNumber;
        SalesforceUrl = salesforceUrl;
        MailingAddress = mailingAddress;
        SourceLastModifiedOn = lastModifiedOn;
        LastSyncedAtUtc = DateTime.UtcNow;

        if (homeCenterChanged)
        {
            Raise(new PartyHomeCenterChangedDomainEvent(homeCenterNumber));
        }

        if (mailingAddressChanged)
        {
            Raise(new PartyMailingAddressChangedDomainEvent());
        }
    }
}
