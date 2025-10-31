using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record NewSessionRequest
{
    [JsonPropertyName("cwd")]
    public required string Cwd { get; init; }

    [JsonPropertyName("mcpServers")]
    public required McpServer[] McpServers { get; init; }

    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }
}
