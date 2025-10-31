using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record AvailableCommand
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("input")]
    public AvailableCommandInput? Input { get; init; }
}
