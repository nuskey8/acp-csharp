using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record AuthenticateRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("methodId")]
    public required string MethodId { get; init; }
}
