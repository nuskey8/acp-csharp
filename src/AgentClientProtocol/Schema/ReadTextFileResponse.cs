using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record ReadTextFileResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}
