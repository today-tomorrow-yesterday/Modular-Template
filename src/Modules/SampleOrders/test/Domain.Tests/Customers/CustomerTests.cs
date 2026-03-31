using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.Customers.Events;
using Xunit;

namespace Modules.SampleOrders.Domain.Tests.Customers;

public sealed class CustomerTests
{
    // ─── Create ───────────────────────────────────────────────────

    [Fact]
    public void Create_returns_success_with_valid_inputs()
    {
        // Arrange & Act
        var result = Customer.Create("John", "M", "Doe", "john@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("John", result.Value.Name.FirstName);
        Assert.Equal("M", result.Value.Name.MiddleName);
        Assert.Equal("Doe", result.Value.Name.LastName);
        Assert.Equal(CustomerStatus.Active, result.Value.Status);
    }

    [Fact]
    public void Create_returns_failure_when_firstName_is_empty()
    {
        // Arrange & Act
        var result = Customer.Create("", null, "Doe", null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NameEmpty, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_returns_failure_when_lastName_is_empty(string? lastName)
    {
        // Arrange & Act
        var result = Customer.Create("John", null, lastName!, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NameEmpty, result.Error);
    }

    [Fact]
    public void Create_adds_email_contact_when_provided()
    {
        // Arrange & Act
        var result = Customer.Create("John", null, "Doe", "john@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        var contact = Assert.Single(result.Value.Contacts);
        Assert.Equal(ContactType.Email, contact.Type);
        Assert.Equal("john@example.com", contact.Value);
        Assert.True(contact.IsPrimary);
    }

    [Fact]
    public void Create_does_not_add_contact_when_email_is_null()
    {
        // Arrange & Act
        var result = Customer.Create("John", null, "Doe", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Contacts);
    }

    [Fact]
    public void Create_generates_PublicId()
    {
        // Arrange & Act
        var result = Customer.Create("John", null, "Doe", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PublicId);
    }

    [Fact]
    public void Create_raises_CustomerCreatedDomainEvent()
    {
        // Arrange & Act
        var result = Customer.Create("John", null, "Doe", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(
            result.Value.DomainEvents,
            e => e is CustomerCreatedDomainEvent);
    }

    // ─── UpdateName ───────────────────────────────────────────────

    [Fact]
    public void UpdateName_changes_name_and_raises_event()
    {
        // Arrange
        var customer = Customer.Create("John", null, "Doe", null).Value;
        customer.ClearDomainEvents();

        // Act
        var result = customer.UpdateName("Jane", "A", "Smith");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Jane", customer.Name.FirstName);
        Assert.Equal("A", customer.Name.MiddleName);
        Assert.Equal("Smith", customer.Name.LastName);
        Assert.Contains(
            customer.DomainEvents,
            e => e is CustomerUpdatedDomainEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateName_returns_failure_when_firstName_is_empty(string? firstName)
    {
        // Arrange
        var customer = Customer.Create("John", null, "Doe", null).Value;

        // Act
        var result = customer.UpdateName(firstName!, null, "Doe");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NameEmpty, result.Error);
    }

    [Fact]
    public void UpdateName_does_not_raise_event_when_name_unchanged()
    {
        // Arrange
        var customer = Customer.Create("John", null, "Doe", null).Value;
        customer.ClearDomainEvents();

        // Act
        var result = customer.UpdateName("John", null, "Doe");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(customer.DomainEvents);
    }

    // ─── AddContact ──────────────────────────────────────────────

    [Fact]
    public void AddContact_adds_contact_and_raises_event()
    {
        // Arrange
        var customer = Customer.Create("John", null, "Doe", null).Value;
        customer.ClearDomainEvents();

        // Act
        customer.AddContact(ContactType.Phone, "555-1234", isPrimary: true);

        // Assert
        var contact = Assert.Single(customer.Contacts);
        Assert.Equal(ContactType.Phone, contact.Type);
        Assert.Equal("555-1234", contact.Value);
        Assert.True(contact.IsPrimary);
        Assert.Contains(
            customer.DomainEvents,
            e => e is CustomerContactsChangedDomainEvent);
    }

    [Fact]
    public void AddContact_demotes_existing_primary_when_new_is_primary()
    {
        // Arrange
        var customer = Customer.Create("John", null, "Doe", "john@example.com").Value;
        // At this point there's one primary email contact

        // Act
        customer.AddContact(ContactType.Email, "jane@example.com", isPrimary: true);

        // Assert
        Assert.Equal(2, customer.Contacts.Count);

        var oldContact = customer.Contacts.First(c => c.Value == "john@example.com");
        var newContact = customer.Contacts.First(c => c.Value == "jane@example.com");

        Assert.False(oldContact.IsPrimary);
        Assert.True(newContact.IsPrimary);
    }
}
