using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record Implementation
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }
}
