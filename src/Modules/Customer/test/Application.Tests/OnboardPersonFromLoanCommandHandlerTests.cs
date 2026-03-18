using Modules.Customer.Application.Parties.OnboardPersonFromLoan;
using Modules.Customer.Domain;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.Customer.Application.Tests;

public sealed class OnboardPersonFromLoanCommandHandlerTests
{
    private readonly IPartyRepository _partyRepository = Substitute.For<IPartyRepository>();
    private readonly IVmfLosAdapter _vmfLosAdapter = Substitute.For<IVmfLosAdapter>();
    private readonly IUnitOfWork<ICustomerModule> _unitOfWork = Substitute.For<IUnitOfWork<ICustomerModule>>();
    private readonly OnboardPersonFromLoanCommandHandler _sut;

    public OnboardPersonFromLoanCommandHandlerTests()
    {
        _sut = new OnboardPersonFromLoanCommandHandler(_partyRepository, _vmfLosAdapter, _unitOfWork);
    }

    [Fact]
    public async Task Returns_existing_PublicId_when_LoanId_already_onboarded()
    {
        var existing = Person.SyncFromCrm(1, 100, LifecycleStage.Customer,
            PersonName.Create("John", null, "Doe"), null, [], null, null, null, null);
        _partyRepository.GetByIdentifierAsync(IdentifierType.LoanId, "LOAN-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.Handle(new OnboardPersonFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.PublicId, result.Value);
        await _vmfLosAdapter.DidNotReceiveWithAnyArgs().GetBorrowerByLoanIdAsync(default!, default);
    }

    [Fact]
    public async Task Returns_BorrowerNotFound_when_VMF_returns_no_data()
    {
        _partyRepository.GetByIdentifierAsync(Arg.Any<IdentifierType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Party?)null);
        _vmfLosAdapter.GetBorrowerByLoanIdAsync("LOAN-1", Arg.Any<CancellationToken>())
            .Returns((VmfLosResponse?)null);

        var result = await _sut.Handle(new OnboardPersonFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Links_LoanId_to_existing_CRM_party_via_ProspectorId()
    {
        _partyRepository.GetByIdentifierAsync(Arg.Any<IdentifierType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Party?)null);

        var existingPerson = Person.SyncFromCrm(42, 100, LifecycleStage.Opportunity,
            PersonName.Create("Existing", null, "Person"), null, [], null, null, null, null);
        _partyRepository.GetForUpdateByIdentifierAsync(
            IdentifierType.CrmPartyId, "42", Arg.Any<CancellationToken>())
            .Returns(existingPerson);

        _vmfLosAdapter.GetBorrowerByLoanIdAsync("LOAN-1", Arg.Any<CancellationToken>())
            .Returns(new VmfLosResponse
            {
                ProspectorId = 42,
                Borrowers = [new VmfBorrower { FirstName = "John", LastName = "Doe" }]
            });

        var result = await _sut.Handle(new OnboardPersonFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingPerson.PublicId, result.Value);
        _partyRepository.Received(1).Update(existingPerson);
        Assert.Equal("LOAN-1", existingPerson.GetIdentifierValue(IdentifierType.LoanId));
    }

    [Fact]
    public async Task Creates_primary_and_coBuyer_when_two_borrowers_returned()
    {
        _partyRepository.GetByIdentifierAsync(Arg.Any<IdentifierType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Party?)null);
        _partyRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Party?)null);

        _vmfLosAdapter.GetBorrowerByLoanIdAsync("LOAN-1", Arg.Any<CancellationToken>())
            .Returns(new VmfLosResponse
            {
                Borrowers =
                [
                    new VmfBorrower { FirstName = "John", LastName = "Doe", Email = "john@test.com", CellPhone = "(555) 123-4567" },
                    new VmfBorrower { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com" }
                ]
            });

        var result = await _sut.Handle(new OnboardPersonFromLoanCommand("LOAN-1", 200), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Two Adds: co-buyer first (needs Id for FK), then primary
        _partyRepository.Received(2).Add(Arg.Any<Party>());
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());

        // Capture the added parties to verify phone cleanup
        var addedParties = new List<Party>();
        _partyRepository.WhenForAnyArgs(x => x.Add(default!))
            .Do(x => addedParties.Add(x.Arg<Party>()));

        // Re-run to capture — but instead just verify via the second Add call
        // The primary person has cleaned phone: "(555) 123-4567" → "5551234567"
        _partyRepository.Received().Add(Arg.Is<Party>(p =>
            ((Person)p).ContactPoints.Any(cp =>
                cp.Type == ContactPointType.MobilePhone && cp.Value == "5551234567")));
    }
}
