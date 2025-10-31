using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record PromptRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("prompt")]
    public required ContentBlock[] Prompt { get; init; }
}
