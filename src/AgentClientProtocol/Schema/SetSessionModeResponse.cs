using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record SetSessionModeResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }
}
