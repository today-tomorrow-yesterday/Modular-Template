using System.Text.Json.Serialization;

namespace Rtl.Core.Infrastructure.ISeries.WireModels;

internal sealed class ODataResponse<T>
{
    [JsonPropertyName("$values")]
    public List<T> Values { get; set; } = [];
}
