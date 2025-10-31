using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record ReadTextFileRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("limit")]
    public uint? Limit { get; init; }

    [JsonPropertyName("line")]
    public uint? Line { get; init; }
}
