using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record SetSessionModelRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("modelId")]
    public required string ModelId { get; init; }
}
