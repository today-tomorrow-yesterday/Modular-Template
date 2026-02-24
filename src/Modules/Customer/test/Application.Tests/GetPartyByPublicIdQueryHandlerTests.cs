using Modules.Customer.Application.Parties.GetPartyByPublicId;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using NSubstitute;
using Xunit;

namespace Modules.Customer.Application.Tests;

public sealed class GetPartyByPublicIdQueryHandlerTests
{
    private readonly IPartyRepository _partyRepository = Substitute.For<IPartyRepository>();
    private readonly GetPartyByPublicIdQueryHandler _sut;

    public GetPartyByPublicIdQueryHandlerTests()
    {
        _sut = new GetPartyByPublicIdQueryHandler(_partyRepository);
    }

    [Fact]
    public async Task Returns_failure_when_party_not_found()
    {
        var publicId = Guid.NewGuid();
        _partyRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Party?)null);

        var result = await _sut.Handle(
            new GetPartyByPublicIdQuery(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Parties.NotFoundByPublicId", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_with_mapped_response_when_party_found()
    {
        var person = Person.SyncFromCrm(
            partyId: 1,
            homeCenterNumber: 100,
            lifecycleStage: LifecycleStage.Customer,
            name: PersonName.Create("John", null, "Doe"),
            dateOfBirth: null,
            salesAssignments: [],
            salesforceUrl: null,
            mailingAddress: null,
            createdOn: null,
            lastModifiedOn: null);

        _partyRepository.GetByPublicIdAsync(person.PublicId, Arg.Any<CancellationToken>())
            .Returns(person);

        var result = await _sut.Handle(
            new GetPartyByPublicIdQuery(person.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(person.PublicId, result.Value.PublicId);
        Assert.Equal("Person", result.Value.PartyType);
        Assert.Equal("Customer", result.Value.LifecycleStage);
        Assert.Equal(100, result.Value.HomeCenterNumber);
    }

    [Fact]
    public async Task Calls_repository_with_correct_public_id()
    {
        var publicId = Guid.NewGuid();
        _partyRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Party?)null);

        await _sut.Handle(new GetPartyByPublicIdQuery(publicId), CancellationToken.None);

        await _partyRepository.Received(1)
            .GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>());
    }
}
