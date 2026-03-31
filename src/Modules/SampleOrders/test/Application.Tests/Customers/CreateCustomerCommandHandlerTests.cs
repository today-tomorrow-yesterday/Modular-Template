using Modules.SampleOrders.Application.Customers.CreateCustomer;
using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.SampleOrders.Application.Tests.Customers;

public sealed class CreateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork<ISampleOrdersModule> _unitOfWork = Substitute.For<IUnitOfWork<ISampleOrdersModule>>();
    private readonly CreateCustomerCommandHandler _sut;

    public CreateCustomerCommandHandlerTests()
    {
        _sut = new CreateCustomerCommandHandler(_customerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Returns_PublicId_on_success()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", "john@example.com");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Adds_customer_to_repository()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", "john@example.com");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _customerRepository.Received(1).Add(Arg.Is<Customer>(c =>
            c.Name.FirstName == "John" &&
            c.Name.LastName == "Doe"));
    }

    [Fact]
    public async Task Calls_SaveChangesAsync_on_success()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null, "Doe", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_failure_when_domain_rejects_creation()
    {
        // Arrange — empty first name will fail domain validation
        var command = new CreateCustomerCommand("", null, "Doe", null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NameEmpty, result.Error);
    }

    [Fact]
    public async Task Does_not_add_to_repository_when_domain_rejects()
    {
        // Arrange
        var command = new CreateCustomerCommand("", null, "Doe", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _customerRepository.DidNotReceive().Add(Arg.Any<Customer>());
    }

    [Fact]
    public async Task Does_not_call_SaveChanges_when_domain_rejects()
    {
        // Arrange
        var command = new CreateCustomerCommand("", null, "Doe", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
