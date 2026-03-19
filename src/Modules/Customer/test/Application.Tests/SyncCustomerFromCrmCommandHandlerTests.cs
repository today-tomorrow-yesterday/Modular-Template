using Modules.Customer.Application.Customers.SyncCustomerFromCrm;
using Modules.Customer.Domain;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.Domain.SalesPersons;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Customer.Application.Tests;

public sealed class SyncCustomerFromCrmCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly ISalesPersonRepository _salesPersonRepository = Substitute.For<ISalesPersonRepository>();
    private readonly IUnitOfWork<ICustomerModule> _unitOfWork = Substitute.For<IUnitOfWork<ICustomerModule>>();
    private readonly SyncCustomerFromCrmCommandHandler _sut;

    public SyncCustomerFromCrmCommandHandlerTests()
    {
        _sut = new SyncCustomerFromCrmCommandHandler(
            _customerRepository,
            _salesPersonRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Creates_new_Customer_with_contact_points_and_identifiers()
    {
        _customerRepository.GetForUpdateByIdentifierAsync(
            IdentifierType.CrmPartyId, "42", Arg.Any<CancellationToken>()).Returns((Domain.Customers.Entities.Customer?)null);
        _salesPersonRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((SalesPerson?)null);

        var command = new SyncCustomerFromCrmCommand(
            CrmPartyId: 42,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Lead,
            FirstName: "John",
            MiddleName: null,
            LastName: "Doe",
            NameExtension: null,
            DateOfBirth: null,
            SalesAssignments: [],
            ContactPoints: [new SyncContactPointDto(ContactPointType.Email, "john@test.com", true)],
            Identifiers: [new SyncIdentifierDto(IdentifierType.SalesforceLeadId, "SF-123")],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: null,
            LastModifiedOn: null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        // CrmPartyId added by factory + 1 explicit identifier + 1 contact point
        _customerRepository.Received(1).Add(Arg.Is<Domain.Customers.Entities.Customer>(c =>
            c.ContactPoints.Count == 1 &&
            c.Identifiers.Count == 2));
    }

    [Fact]
    public async Task Updates_existing_Customer_and_replaces_contact_points()
    {
        var existing = Domain.Customers.Entities.Customer.SyncFromCrm(42, 100, LifecycleStage.Lead,
            CustomerName.Create("John", null, "Doe"), null, [], null, null, null, null);
        existing.ReplaceContactPoints([(ContactPointType.Email, "old@test.com", true)]);
        existing.ClearDomainEvents();

        _customerRepository.GetForUpdateByIdentifierAsync(
            IdentifierType.CrmPartyId, "42", Arg.Any<CancellationToken>()).Returns(existing);

        var command = new SyncCustomerFromCrmCommand(
            CrmPartyId: 42,
            HomeCenterNumber: 200, // Changed
            LifecycleStage: LifecycleStage.Lead,
            FirstName: "John",
            MiddleName: "Q",
            LastName: "Doe",
            NameExtension: null,
            DateOfBirth: null,
            SalesAssignments: [],
            ContactPoints: [new SyncContactPointDto(ContactPointType.Email, "new@test.com", true)], // Changed
            Identifiers: [],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: null,
            LastModifiedOn: null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _customerRepository.DidNotReceiveWithAnyArgs().Add(Arg.Any<Domain.Customers.Entities.Customer>()); // Update, not create

        // Verify domain events raised for the actual changes
        Assert.Contains(existing.DomainEvents, e => e is CustomerNameChangedDomainEvent);
        Assert.Contains(existing.DomainEvents, e => e is CustomerHomeCenterChangedDomainEvent);
        Assert.Contains(existing.DomainEvents, e => e is CustomerContactPointsChangedDomainEvent);
    }

    [Fact]
    public async Task Upserts_SalesPersons_before_creating_Customer()
    {
        _customerRepository.GetForUpdateByIdentifierAsync(
            IdentifierType.CrmPartyId, "42", Arg.Any<CancellationToken>()).Returns((Domain.Customers.Entities.Customer?)null);

        var existingSp = SalesPerson.Assign("SP-1", "old@test.com", "olduser", "Old", "Name", null, "FED-1");
        _salesPersonRepository.GetByIdAsync("SP-1", Arg.Any<CancellationToken>()).Returns(existingSp);
        _salesPersonRepository.GetByIdAsync("SP-2", Arg.Any<CancellationToken>()).Returns((SalesPerson?)null);

        var command = new SyncCustomerFromCrmCommand(
            CrmPartyId: 42,
            HomeCenterNumber: 100,
            LifecycleStage: LifecycleStage.Lead,
            FirstName: "John",
            MiddleName: null,
            LastName: "Doe",
            NameExtension: null,
            DateOfBirth: null,
            SalesAssignments:
            [
                new SyncSalesAssignmentDto(SalesAssignmentRole.Primary, new SyncSalesPersonDto("SP-1", "new@test.com", "newuser", "New", "Name", 5, "FED-1")),
                new SyncSalesAssignmentDto(SalesAssignmentRole.Supporting, new SyncSalesPersonDto("SP-2", "sp2@test.com", "sp2user", "Sales", "Two", null, "FED-2"))
            ],
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
}
