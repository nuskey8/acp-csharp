using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record ToolCallLocation
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("line")]
    public uint? Line { get; init; }
}
