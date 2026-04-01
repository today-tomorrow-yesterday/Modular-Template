using Newtonsoft.Json;

namespace ModularTemplate.Infrastructure.EventBus.Aws;

/// <summary>
/// Represents the envelope structure of an EventBridge event delivered via SQS.
/// </summary>
/// <remarks>
/// <para>
/// When EventBridge delivers events to SQS, they are wrapped in an envelope that contains
/// metadata about the event such as the source, detail-type, timestamp, and the actual
/// event payload in the Detail field.
/// </para>
/// <para>
/// The detail-type field uses a hyphen which requires special JSON property mapping.
/// </para>
/// </remarks>
internal sealed record EventBridgeEnvelope(
    string Source,
    [property: JsonProperty("detail-type")] string DetailType,
    string Detail,
    DateTime Time);
