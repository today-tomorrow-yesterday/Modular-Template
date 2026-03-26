using Modules.Sales.Application.Sales.GetSaleById;
using Modules.Sales.Domain.CustomersCache;
using RetailLocationCacheEntity = Modules.Sales.Domain.RetailLocationCache.RetailLocationCache;
using Modules.Sales.Domain.Sales;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace Modules.Sales.Application.Tests.Sales;

public sealed class GetSaleByIdQueryHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly GetSaleByIdQueryHandler _sut;

    private static readonly Guid TestSalePublicId = Guid.NewGuid();

    public GetSaleByIdQueryHandlerTests()
    {
        _sut = new GetSaleByIdQueryHandler(_saleRepository);
    }

    [Fact]
    public async Task Returns_failure_when_sale_not_found()
    {
        _saleRepository.GetByPublicIdWithCustomerContextAsync(TestSalePublicId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(TestSalePublicId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Sale.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Returns_correct_sale_identifiers()
    {
        var sale = CreateSaleWithContext();
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(sale.PublicId, result.Value.Id);
        // SaleNumber is DB-generated (identity column), so it's 0 in unit tests
        Assert.Equal(0, result.Value.SaleNumber);
    }

    [Fact]
    public async Task Returns_correct_sale_type_and_status()
    {
        var sale = CreateSaleWithContext();
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("B2C", result.Value.SaleType);
        Assert.Equal("Inquiry", result.Value.SaleStatus);
    }

    [Fact]
    public async Task Returns_correct_retail_location()
    {
        var sale = CreateSaleWithContext();
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var location = result.Value.RetailLocation;
        Assert.Equal("HomeCenter", location.Type);
        Assert.Equal("Test HC", location.Name);
        Assert.Equal("OH", location.StateCode);
        Assert.Equal("43004", location.Zip);
        Assert.Equal(42, location.HomeCenterNumber);
    }

    [Fact]
    public async Task Returns_correct_customer_data()
    {
        var sale = CreateSaleWithContext(
            firstName: "Jane",
            middleName: "Marie",
            lastName: "Smith",
            email: "jane@test.com",
            phone: "555-1234");
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var customer = result.Value.Customer;
        Assert.Equal("Jane", customer.FirstName);
        Assert.Equal("Marie", customer.MiddleName);
        Assert.Equal("Smith", customer.LastName);
        Assert.Equal("jane@test.com", customer.Email);
        Assert.Equal("555-1234", customer.Phone);
    }

    [Fact]
    public async Task Returns_display_name_as_first_name_when_name_fields_empty()
    {
        var sale = CreateSaleWithContext(includePerson: false, displayName: "Acme Corp");
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Acme Corp", result.Value.Customer.FirstName);
        Assert.Null(result.Value.Customer.MiddleName);
        Assert.Equal(string.Empty, result.Value.Customer.LastName);
    }

    [Fact]
    public async Task Returns_correct_customer_public_id()
    {
        var customerPublicId = Guid.NewGuid();
        var sale = CreateSaleWithContext(customerPublicId: customerPublicId);
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customerPublicId, result.Value.CustomerId);
    }

    [Fact]
    public async Task Returns_correct_cobuyer_data()
    {
        var sale = CreateSaleWithContext(
            coBuyerFirstName: "Bob",
            coBuyerLastName: "Jones");
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Bob", result.Value.Customer.CoBuyerFirstName);
        Assert.Equal("Jones", result.Value.Customer.CoBuyerLastName);
    }

    [Fact]
    public async Task Returns_correct_salesperson_data()
    {
        var sale = CreateSaleWithContext(
            primarySpFederatedId: "fed-001",
            primarySpFirstName: "Tom",
            primarySpLastName: "Primary",
            secondarySpFederatedId: "fed-002",
            secondarySpFirstName: "Sue",
            secondarySpLastName: "Secondary");
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var customer = result.Value.Customer;
        Assert.Equal("fed-001", customer.PrimarySalesPersonFederatedId);
        Assert.Equal("Tom", customer.PrimarySalesPersonFirstName);
        Assert.Equal("Primary", customer.PrimarySalesPersonLastName);
        Assert.Equal("fed-002", customer.SecondarySalesPersonFederatedId);
        Assert.Equal("Sue", customer.SecondarySalesPersonFirstName);
        Assert.Equal("Secondary", customer.SecondarySalesPersonLastName);
    }

    [Fact]
    public async Task Returns_correct_home_center_number_from_customer()
    {
        var sale = CreateSaleWithContext(customerHomeCenterNumber: 99);
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.Value.Customer.HomeCenterNumber);
    }

    [Fact]
    public async Task Returns_salesforce_id_from_customer()
    {
        var sale = CreateSaleWithContext(salesforceAccountId: "001ABC");
        SetupRepository(sale);

        var result = await _sut.Handle(
            new GetSaleByIdQuery(sale.PublicId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("001ABC", result.Value.Customer.SalesforceId);
    }

    // --- Test helpers ---

    private void SetupRepository(Sale sale)
    {
        _saleRepository.GetByPublicIdWithCustomerContextAsync(sale.PublicId, Arg.Any<CancellationToken>())
            .Returns(sale);
    }

    private static Sale CreateSaleWithContext(
        bool includePerson = true,
        string displayName = "Test Customer",
        string firstName = "Test",
        string? middleName = null,
        string lastName = "Customer",
        string? email = null,
        string? phone = null,
        string? coBuyerFirstName = null,
        string? coBuyerLastName = null,
        string? primarySpFederatedId = null,
        string? primarySpFirstName = null,
        string? primarySpLastName = null,
        string? secondarySpFederatedId = null,
        string? secondarySpFirstName = null,
        string? secondarySpLastName = null,
        Guid? customerPublicId = null,
        int customerHomeCenterNumber = 42,
        string? salesforceAccountId = null)
    {
        var sale = Sale.Create(
            customerId: 1,
            retailLocationId: 1,
            saleType: SaleType.B2C);
        sale.ClearDomainEvents();

        // Build flat customer cache
        var customer = new CustomerCache
        {
            Id = 1,
            RefPublicId = customerPublicId ?? Guid.NewGuid(),
            HomeCenterNumber = customerHomeCenterNumber,
            DisplayName = displayName,
            SalesforceAccountId = salesforceAccountId,
            FirstName = includePerson ? firstName : displayName,
            MiddleName = includePerson ? middleName : null,
            LastName = includePerson ? lastName : string.Empty,
            Email = email,
            Phone = phone,
            CoBuyerFirstName = coBuyerFirstName,
            CoBuyerLastName = coBuyerLastName,
            PrimarySalesPersonFederatedId = primarySpFederatedId,
            PrimarySalesPersonFirstName = primarySpFirstName,
            PrimarySalesPersonLastName = primarySpLastName,
            SecondarySalesPersonFederatedId = secondarySpFederatedId,
            SecondarySalesPersonFirstName = secondarySpFirstName,
            SecondarySalesPersonLastName = secondarySpLastName
        };

        SetProperty(sale, nameof(Sale.Customer), customer);

        // Set RetailLocation navigation via reflection
        var retailLocation = RetailLocationCacheEntity.CreateHomeCenter(
            homeCenterNumber: 42, name: "Test HC", stateCode: "OH", zip: "43004", isActive: true);
        SetProperty(sale, nameof(Sale.RetailLocation), retailLocation);

        return sale;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField is not null)
        {
            backingField.SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(obj, value);
        }
    }
}
