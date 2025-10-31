using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record AuthMethod
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
