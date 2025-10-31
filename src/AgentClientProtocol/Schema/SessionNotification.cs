using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record SessionNotification
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("update")]
    public required SessionUpdate Update { get; init; }
}
