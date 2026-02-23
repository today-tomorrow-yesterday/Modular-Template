using Modules.Customer.Application.Parties.SyncPartyFromCrm;
using Modules.Customer.Domain;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.Domain.SalesPersons;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Customer.Application.Tests;

public sealed class SyncPartyFromCrmCommandHandlerTests
{
    private readonly IPartyRepository _partyRepository = Substitute.For<IPartyRepository>();
    private readonly ISalesPersonRepository _salesPersonRepository = Substitute.For<ISalesPersonRepository>();
    private readonly IUnitOfWork<ICustomerModule> _unitOfWork = Substitute.For<IUnitOfWork<ICustomerModule>>();
    private readonly SyncPartyFromCrmCommandHandler _sut;

    public SyncPartyFromCrmCommandHandlerTests()
    {
        _sut = new SyncPartyFromCrmCommandHandler(_partyRepository, _salesPersonRepository, _unitOfWork);
    }

    [Fact]
    public async Task Creates_new_Person_with_contact_points_and_identifiers()
    {
        _partyRepository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Party?)null);
        _salesPersonRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((SalesPerson?)null);

        var command = new SyncPartyFromCrmCommand(
            PartyId: 42,
            PartyType: PartyType.Person,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Lead,
            PersonData: new SyncPersonDataDto("John", null, "Doe", null, null, []),
            OrganizationData: null,
            ContactPoints: [new SyncContactPointDto(ContactPointType.Email, "john@test.com", true)],
            Identifiers: [new SyncIdentifierDto(IdentifierType.SalesforceLeadId, "SF-123")],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: null,
            LastModifiedOn: null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _partyRepository.Received(1).Add(Arg.Is<Party>(p =>
            ((Person)p).ContactPoints.Count == 1 &&
            ((Person)p).Identifiers.Count == 1));
    }

    [Fact]
    public async Task Updates_existing_Person_and_replaces_contact_points()
    {
        var existing = Person.SyncFromCrm(42, 100, LifecycleStage.Lead,
            PersonName.Create("John", null, "Doe"), null, [], null, null, null, null);
        existing.ReplaceContactPoints([(ContactPointType.Email, "old@test.com", true)]);
        existing.ClearDomainEvents();

        _partyRepository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns(existing);

        var command = new SyncPartyFromCrmCommand(
            PartyId: 42,
            PartyType: PartyType.Person,
            HomeCenterNumber: 200, // Changed
            LifecycleStage: LifecycleStage.Lead,
            PersonData: new SyncPersonDataDto("John", "Q", "Doe", null, null, []), // Name changed
            OrganizationData: null,
            ContactPoints: [new SyncContactPointDto(ContactPointType.Email, "new@test.com", true)], // Changed
            Identifiers: [],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: null,
            LastModifiedOn: null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _partyRepository.DidNotReceiveWithAnyArgs().Add(Arg.Any<Party>()); // Update, not create

        // Verify domain events raised for the actual changes
        Assert.Contains(existing.DomainEvents, e => e is PartyNameChangedDomainEvent);
        Assert.Contains(existing.DomainEvents, e => e is PartyHomeCenterChangedDomainEvent);
        Assert.Contains(existing.DomainEvents, e => e is PartyContactPointsChangedDomainEvent);
    }

    [Fact]
    public async Task Upserts_SalesPersons_before_creating_Person()
    {
        _partyRepository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Party?)null);

        var existingSp = SalesPerson.Assign("SP-1", "old@test.com", "olduser", "Old", "Name", null, "FED-1");
        _salesPersonRepository.GetByIdAsync("SP-1", Arg.Any<CancellationToken>()).Returns(existingSp);
        _salesPersonRepository.GetByIdAsync("SP-2", Arg.Any<CancellationToken>()).Returns((SalesPerson?)null);

        var command = new SyncPartyFromCrmCommand(
            PartyId: 42,
            PartyType: PartyType.Person,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Lead,
            PersonData: new SyncPersonDataDto("John", null, "Doe", null, null,
            [
                new SyncSalesAssignmentDto(SalesAssignmentRole.Primary, new SyncSalesPersonDto("SP-1", "new@test.com", "newuser", "New", "Name", 5, "FED-1")),
                new SyncSalesAssignmentDto(SalesAssignmentRole.Supporting, new SyncSalesPersonDto("SP-2", "sp2@test.com", "sp2user", "Sales", "Two", null, "FED-2"))
            ]),
            OrganizationData: null,
            ContactPoints: [],
            Identifiers: [],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: null,
            LastModifiedOn: null);

        await _sut.Handle(command, CancellationToken.None);

        // SP-1 updated (existed), SP-2 added (new)
        _salesPersonRepository.Received(1).Add(Arg.Is<SalesPerson>(sp => sp.Email == "sp2@test.com"));
        Assert.Equal("new@test.com", existingSp.Email); // SP-1 was updated in place
    }

    [Fact]
    public async Task Creates_Organization_with_full_CDC_payload()
    {
        _partyRepository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Party?)null);

        var command = new SyncPartyFromCrmCommand(
            PartyId: 99,
            PartyType: PartyType.Organization,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Customer,
            PersonData: null,
            OrganizationData: new SyncOrganizationDataDto("Acme Homes LLC"),
            ContactPoints: [new SyncContactPointDto(ContactPointType.Phone, "555-0000", true)],
            Identifiers: [new SyncIdentifierDto(IdentifierType.SalesforceAccountId, "SF-ACCT-1")],
            MailingAddress: new SyncMailingAddressDto("123 Main", null, "Dallas", null, "TX", "US", "75201"),
            SalesforceUrl: "https://sf.example.com",
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _partyRepository.Received(1).Add(Arg.Is<Party>(p =>
            ((Organization)p).OrganizationName == "Acme Homes LLC" &&
            p.MailingAddress != null &&
            p.ContactPoints.Count == 1 &&
            p.Identifiers.Count == 1));
    }

    [Fact]
    public async Task Returns_failure_for_mismatched_party_type_data()
    {
        _partyRepository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Party?)null);

        var command = new SyncPartyFromCrmCommand(
            PartyId: 42,
            PartyType: PartyType.Person,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Lead,
            PersonData: null, // Person type but no PersonData
            OrganizationData: null,
            ContactPoints: [],
            Identifiers: [],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: null,
            LastModifiedOn: null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
