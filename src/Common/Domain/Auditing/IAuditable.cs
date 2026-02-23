namespace Rtl.Core.Domain.Auditing;

/// <summary>
/// Marker interface for entities that should have field-level audit tracking.
/// Entities implementing this (or inheriting from AuditableEntity) will have
/// their changes logged to the audit_logs table.
/// </summary>
public interface IAuditable;
