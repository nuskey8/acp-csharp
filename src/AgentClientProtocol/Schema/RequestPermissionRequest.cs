using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record RequestPermissionRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("toolCall")]
    public required object ToolCall { get; init; }

    [JsonPropertyName("options")]
    public required PermissionOption[] Options { get; init; }
}
