using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record PermissionOption
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("optionId")]
    public required string OptionId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("kind")]
    public required PermissionOptionKind Kind { get; init; }
}
