using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record SetSessionModeRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }
}
