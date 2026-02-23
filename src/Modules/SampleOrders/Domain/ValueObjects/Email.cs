using Rtl.Core.Domain.Results;
using System.Text.RegularExpressions;

namespace Modules.SampleOrders.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address.
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    private const int MaxLength = 255;

    // Private parameterless constructor for EF Core
    private Email()
    {
        Value = string.Empty;
    }

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a new Email value object with validation.
    /// </summary>
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(EmailErrors.Empty);
        }

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
        {
            return Result.Failure<Email>(EmailErrors.TooLong);
        }

        if (!EmailRegex().IsMatch(email))
        {
            return Result.Failure<Email>(EmailErrors.InvalidFormat);
        }

        return new Email(email);
    }

    /// <summary>
    /// Internal factory for EF Core materialization - bypasses validation.
    /// </summary>
    internal static Email FromDatabase(string value)
    {
        return new Email(value);
    }

    public static implicit operator string(Email email) => email.Value;

    public override bool Equals(object? obj) => Equals(obj as Email);

    public bool Equals(Email? other)
    {
        if (other is null)
        {
            return false;
        }

        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Email? left, Email? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Email? left, Email? right) => !(left == right);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
