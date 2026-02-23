using Rtl.Core.Domain.Auditing;

namespace Rtl.Core.Domain.Entities;

/// <summary>
/// Interface for entities that track audit information.
/// Entities implementing this interface will have their audit fields
/// automatically populated by the AuditableEntitiesInterceptor.
/// </summary>
public interface IAuditableEntity : IAuditable
{
    Guid CreatedByUserId { get; }

    DateTime CreatedAtUtc { get; }

    Guid? ModifiedByUserId { get; }

    DateTime? ModifiedAtUtc { get; }
}
