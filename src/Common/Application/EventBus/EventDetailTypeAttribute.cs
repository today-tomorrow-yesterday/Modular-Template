namespace Rtl.Core.Application.EventBus;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EventDetailTypeAttribute(string detailType) : Attribute
{
    public string DetailType { get; } = detailType;
}
