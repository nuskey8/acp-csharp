using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record PromptResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("stopReason")]
    public required StopReason StopReason { get; init; }
}
