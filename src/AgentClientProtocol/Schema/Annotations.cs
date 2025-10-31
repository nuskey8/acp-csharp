using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record Annotations
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("audience")]
    public Role[]? Audience { get; init; }

    [JsonPropertyName("lastModified")]
    public string? LastModified { get; init; }

    [JsonPropertyName("priority")]
    public double? Priority { get; init; }
}
