using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Domain.ValueObjects;

public static class EmailErrors
{
    public static readonly Error Empty =
        Error.Validation("Email.Empty", "Email address cannot be empty.");

    public static readonly Error InvalidFormat =
        Error.Validation("Email.InvalidFormat", "Email address format is invalid.");

    public static readonly Error TooLong =
        Error.Validation("Email.TooLong", "Email address cannot exceed 255 characters.");
}
