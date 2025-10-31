using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record LoadSessionResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("models")]
    public SessionModelState? Models { get; init; }

    [JsonPropertyName("modes")]
    public SessionModeState? Modes { get; init; }
}
