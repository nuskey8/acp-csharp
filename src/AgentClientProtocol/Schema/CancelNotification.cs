using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record CancelNotification
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }
}
