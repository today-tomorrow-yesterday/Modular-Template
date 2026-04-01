using System.Reflection;

namespace ModularTemplate.Application.EventBus;

/// <summary>
/// Base record for integration events.
/// </summary>
public abstract record IntegrationEvent(Guid Id, DateTime OccurredOnUtc) : IIntegrationEvent
{
    /// <summary>
    /// Returns this event's detail-type string (e.g., "mt.sales.saleSummaryChanged").
    /// </summary>
    public string GetEventDetailType() => GetDetailType(GetType());

    /// <summary>
    /// Returns the detail-type string for the given event type by reading its
    /// <see cref="EventDetailTypeAttribute"/>. Falls back to AssemblyQualifiedName
    /// if the attribute is absent.
    /// </summary>
    public static string GetDetailType(Type eventType)
    {
        var attr = eventType.GetCustomAttribute<EventDetailTypeAttribute>();
        return attr?.DetailType ?? eventType.AssemblyQualifiedName!;
    }
}
