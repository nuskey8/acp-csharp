using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record WriteTextFileResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }
}
