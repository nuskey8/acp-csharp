using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record EnvVariable
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}
