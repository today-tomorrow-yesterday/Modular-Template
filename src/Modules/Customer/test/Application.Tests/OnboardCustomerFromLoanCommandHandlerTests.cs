using Modules.Customer.Application.Customers.OnboardCustomerFromLoan;
using Modules.Customer.Domain;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Customer.Application.Tests;

public sealed class OnboardCustomerFromLoanCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IVmfLosAdapter _vmfLosAdapter = Substitute.For<IVmfLosAdapter>();
    private readonly IUnitOfWork<ICustomerModule> _unitOfWork = Substitute.For<IUnitOfWork<ICustomerModule>>();
    private readonly OnboardCustomerFromLoanCommandHandler _sut;

    public OnboardCustomerFromLoanCommandHandlerTests()
    {
        _sut = new OnboardCustomerFromLoanCommandHandler(_customerRepository, _vmfLosAdapter, _unitOfWork);
    }

    [Fact]
    public async Task Returns_existing_PublicId_when_LoanId_already_onboarded()
    {
        var existing = Domain.Customers.Entities.Customer.SyncFromCrm(crmCustomerId: 1, 100, LifecycleStage.Customer,
            CustomerName.Create("John", null, "Doe"), null, [], null, null, null, null);
        _customerRepository.GetByIdentifierAsync(IdentifierType.LoanId, "LOAN-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.Handle(new OnboardCustomerFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.PublicId, result.Value);
        await _vmfLosAdapter.DidNotReceiveWithAnyArgs().GetBorrowerByLoanIdAsync(default!, default);
    }

    [Fact]
    public async Task Returns_BorrowerNotFound_when_VMF_returns_no_data()
    {
        _customerRepository.GetByIdentifierAsync(Arg.Any<IdentifierType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Customers.Entities.Customer?)null);
        _vmfLosAdapter.GetBorrowerByLoanIdAsync("LOAN-1", Arg.Any<CancellationToken>())
            .Returns((VmfLosResponse?)null);

        var result = await _sut.Handle(new OnboardCustomerFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Links_LoanId_to_existing_CRM_customer_via_ProspectorId()
    {
        _customerRepository.GetByIdentifierAsync(Arg.Any<IdentifierType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Customers.Entities.Customer?)null);

        var existingCustomer = Domain.Customers.Entities.Customer.SyncFromCrm(crmCustomerId: 42, 100, LifecycleStage.Opportunity,
            CustomerName.Create("Existing", null, "Person"), null, [], null, null, null, null);
        _customerRepository.GetForUpdateByIdentifierAsync(
            IdentifierType.CrmCustomerId, "42", Arg.Any<CancellationToken>())
            .Returns(existingCustomer);

        _vmfLosAdapter.GetBorrowerByLoanIdAsync("LOAN-1", Arg.Any<CancellationToken>())
            .Returns(new VmfLosResponse
            {
                ProspectorId = 42,
                Borrowers = [new VmfBorrower { FirstName = "John", LastName = "Doe" }]
            });

        var result = await _sut.Handle(new OnboardCustomerFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingCustomer.PublicId, result.Value);
        _customerRepository.Received(1).Update(existingCustomer);
        Assert.Equal("LOAN-1", existingCustomer.GetIdentifierValue(IdentifierType.LoanId));
    }

    [Fact]
    public async Task Creates_primary_and_coBuyer_when_two_borrowers_returned()
    {
        _customerRepository.GetByIdentifierAsync(Arg.Any<IdentifierType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Customers.Entities.Customer?)null);
        _customerRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Customers.Entities.Customer?)null);

        _vmfLosAdapter.GetBorrowerByLoanIdAsync("LOAN-1", Arg.Any<CancellationToken>())
            .Returns(new VmfLosResponse
            {
                Borrowers =
                [
                    new VmfBorrower { FirstName = "John", LastName = "Doe", Email = "john@test.com", CellPhone = "(555) 123-4567" },
                    new VmfBorrower { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com" }
                ]
            });

        var result = await _sut.Handle(new OnboardCustomerFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Two Adds: co-buyer first (needs Id for FK), then primary
        _customerRepository.Received(2).Add(Arg.Any<Domain.Customers.Entities.Customer>());
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());

        // The primary person has cleaned phone: "(555) 123-4567" → "5551234567"
        _customerRepository.Received().Add(Arg.Is<Domain.Customers.Entities.Customer>(c =>
            c.ContactPoints.Any(cp =>
                cp.Type == ContactPointType.MobilePhone && cp.Value == "5551234567")));
    }
}
