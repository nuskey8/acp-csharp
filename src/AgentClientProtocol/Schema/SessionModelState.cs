using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record SessionModelState
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("currentModelId")]
    public required string CurrentModelId { get; init; }

    [JsonPropertyName("availableModels")]
    public required ModelInfo[] AvailableModels { get; init; }
}
