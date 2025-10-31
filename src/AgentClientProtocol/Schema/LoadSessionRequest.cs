using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record LoadSessionRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("mcpServers")]
    public required McpServer[] McpServers { get; init; }

    [JsonPropertyName("cwd")]
    public required string Cwd { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }
}
