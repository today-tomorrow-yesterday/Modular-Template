// Placeholder types for CMH.Common.EMBClient.
// Delete this file and add the real NuGet package when available.

namespace CMH.Common.EMBClient.Contracts;

public interface IEMBProducer
{
    Task SendEventAsync<T>(string source, string detailType, EMBMessage<T> message);
}

public class EMBMessage<T>
{
    public T Data { get; set; } = default!;
    public EMBMessageMetadata Metadata { get; set; } = new();
}

public class EMBMessageMetadata
{
    public string MessageId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool ForwardToEmb { get; set; }
    public string Reporter { get; set; } = string.Empty;
    public string? Environment { get; set; }
}
