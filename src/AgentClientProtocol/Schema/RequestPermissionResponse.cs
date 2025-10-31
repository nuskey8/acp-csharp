using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record RequestPermissionResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("outcome")]
    public required RequestPermissionOutcome Outcome { get; init; }
}
