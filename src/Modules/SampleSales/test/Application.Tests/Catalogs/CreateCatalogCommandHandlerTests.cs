using Modules.SampleSales.Application.Catalogs.CreateCatalog;
using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Catalogs;
using NSubstitute;
using Rtl.Core.Application.Persistence;
using Xunit;

namespace Modules.SampleSales.Application.Tests.Catalogs;

public sealed class CreateCatalogCommandHandlerTests
{
    private readonly ICatalogRepository _catalogRepository = Substitute.For<ICatalogRepository>();
    private readonly IUnitOfWork<ISampleSalesModule> _unitOfWork = Substitute.For<IUnitOfWork<ISampleSalesModule>>();
    private readonly CreateCatalogCommandHandler _sut;

    public CreateCatalogCommandHandlerTests()
    {
        _sut = new CreateCatalogCommandHandler(_catalogRepository, _unitOfWork);
    }

    [Fact]
    public async Task Returns_PublicId_on_success()
    {
        // Arrange
        var command = new CreateCatalogCommand("Summer Collection", "Summer 2026 products");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Adds_catalog_to_repository()
    {
        // Arrange
        var command = new CreateCatalogCommand("Summer Collection", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _catalogRepository.Received(1).Add(Arg.Is<Catalog>(c =>
            c.Name == "Summer Collection"));
    }

    [Fact]
    public async Task Calls_SaveChangesAsync_on_success()
    {
        // Arrange
        var command = new CreateCatalogCommand("Summer Collection", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_failure_when_domain_rejects_creation()
    {
        // Arrange — empty name will fail domain validation
        var command = new CreateCatalogCommand("", null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CatalogErrors.NameEmpty, result.Error);
    }

    [Fact]
    public async Task Does_not_add_to_repository_when_domain_rejects()
    {
        // Arrange
        var command = new CreateCatalogCommand("", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _catalogRepository.DidNotReceive().Add(Arg.Any<Catalog>());
    }

    [Fact]
    public async Task Does_not_call_SaveChanges_when_domain_rejects()
    {
        // Arrange
        var command = new CreateCatalogCommand("", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
