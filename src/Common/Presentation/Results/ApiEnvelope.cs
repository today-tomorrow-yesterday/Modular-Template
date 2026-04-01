using System.Text.Json.Serialization;

namespace ModularTemplate.Presentation.Results;

/// <summary>
/// Uniform API response envelope. Every endpoint returns this shape.
/// </summary>
public sealed class ApiEnvelope<T>
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("problemDetails")]
    public ApiProblemEnvelope? ProblemDetails { get; init; }
}

/// <summary>
/// RFC 9457 Problem Details with errors list, requestId, and traceId.
/// </summary>
public sealed class ApiProblemEnvelope
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("status")]
    public required int Status { get; init; }

    [JsonPropertyName("instance")]
    public required string Instance { get; init; }

    [JsonPropertyName("requestId")]
    public required string RequestId { get; init; }

    [JsonPropertyName("traceId")]
    public required string TraceId { get; init; }

    [JsonPropertyName("errors")]
    public required IReadOnlyList<ApiErrorDetail> Errors { get; init; }
}

/// <summary>
/// Individual error entry inside ProblemDetails.
/// </summary>
public sealed class ApiErrorDetail
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }
}
