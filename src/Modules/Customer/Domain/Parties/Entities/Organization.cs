using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;

namespace Modules.Customer.Domain.Parties.Entities;

// Organization party — business entity (developer group, company) in SES Pro.
// ~5% of parties. No CoBuyer, no SalesPersons, no DateOfBirth.
// Enterprise has akaNames[] for trade names — retail needs only OrganizationName.
public sealed class Organization : Party
{
    private Organization() => PartyType = PartyType.Organization;

    public string? OrganizationName { get; private set; }

    // ─── Factory Methods ───────────────────────────────────────────

    public static Organization CreateLead(
        int homeCenterNumber,
        string? organizationName,
        string salesforceLeadId,
        string? salesforceUrl)
    {
        var org = new Organization
        {
            PublicId = Guid.CreateVersion7(),
            LifecycleStage = LifecycleStage.Lead,
            OrganizationName = organizationName,
            HomeCenterNumber = homeCenterNumber,
            SalesforceUrl = salesforceUrl,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        org.AddIdentifierInternal(IdentifierType.SalesforceLeadId, salesforceLeadId);
        org.Raise(new PartyCreatedDomainEvent());

        return org;
    }

    public static Organization SyncFromCrm(
        int partyId,
        int homeCenterNumber,
        LifecycleStage lifecycleStage,
        string? organizationName,
        string? salesforceUrl,
        MailingAddress? mailingAddress,
        DateTimeOffset? createdOn,
        DateTimeOffset? lastModifiedOn)
    {
        var org = new Organization
        {
            Id = partyId,
            PublicId = Guid.CreateVersion7(),
            LifecycleStage = lifecycleStage,
            OrganizationName = organizationName,
            HomeCenterNumber = homeCenterNumber,
            SalesforceUrl = salesforceUrl,
            MailingAddress = mailingAddress,
            SourceCreatedOn = createdOn,
            SourceLastModifiedOn = lastModifiedOn,
            LastSyncedAtUtc = DateTime.UtcNow
        };

        org.Raise(new PartyCreatedDomainEvent());

        return org;
    }

    // ─── Behavioral Methods ────────────────────────────────────────

    public void UpdateFromCrmSync(
        string? organizationName,
        int homeCenterNumber,
        string? salesforceUrl,
        MailingAddress? mailingAddress,
        DateTimeOffset? lastModifiedOn)
    {
        var nameChanged = OrganizationName != organizationName;
        OrganizationName = organizationName;

        ApplySharedCrmSyncFields(homeCenterNumber, salesforceUrl, mailingAddress, lastModifiedOn);

        if (nameChanged)
        {
            Raise(new PartyNameChangedDomainEvent());
        }
    }
}
