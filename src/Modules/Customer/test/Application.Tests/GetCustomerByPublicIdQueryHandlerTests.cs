using Modules.Customer.Application.Customers.GetCustomerByPublicId;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using NSubstitute;
using Xunit;

namespace Modules.Customer.Application.Tests;

public sealed class GetCustomerByPublicIdQueryHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly GetCustomerByPublicIdQueryHandler _sut;

    public GetCustomerByPublicIdQueryHandlerTests()
    {
        _sut = new GetCustomerByPublicIdQueryHandler(_customerRepository);
    }

    [Fact]
    public async Task Returns_failure_when_customer_not_found()
    {
        var publicId = Guid.NewGuid();
        _customerRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Domain.Customers.Entities.Customer?)null);

        var result = await _sut.Handle(
            new GetCustomerByPublicIdQuery(publicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Customers.NotFoundByPublicId", result.Error.Code);
    }

    [Fact]
    public async Task Returns_success_with_mapped_response_when_customer_found()
    {
        var customer = Domain.Customers.Entities.Customer.SyncFromCrm(
            crmCustomerId: 1,
            homeCenterNumber: 100,
            lifecycleStage: LifecycleStage.Customer,
            name: CustomerName.Create("John", null, "Doe"),
            dateOfBirth: null,
            salesAssignments: [],
            salesforceUrl: null,
            mailingAddress: null,
            createdOn: null,
            lastModifiedOn: null);

        _customerRepository.GetByPublicIdAsync(customer.PublicId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var result = await _sut.Handle(
            new GetCustomerByPublicIdQuery(customer.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customer.PublicId, result.Value.PublicId);
        Assert.Equal("Customer", result.Value.LifecycleStage);
        Assert.Equal(100, result.Value.HomeCenterNumber);
        Assert.Equal("John", result.Value.FirstName);
        Assert.Equal("Doe", result.Value.LastName);
    }

    [Fact]
    public async Task Calls_repository_with_correct_public_id()
    {
        var publicId = Guid.NewGuid();
        _customerRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((Domain.Customers.Entities.Customer?)null);

        await _sut.Handle(new GetCustomerByPublicIdQuery(publicId), CancellationToken.None);

        await _customerRepository.Received(1)
            .GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>());
    }
}
