using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record SessionModeState
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("currentModeId")]
    public required string CurrentModeId { get; init; }

    [JsonPropertyName("availableModes")]
    public required SessionMode[] AvailableModes { get; init; }
}
