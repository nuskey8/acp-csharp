using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record AgentCapabilities
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("loadSession")]
    public bool LoadSession { get; init; } = false;

    [JsonPropertyName("mcpCapabilities")]
    public McpCapabilities McpCapabilities { get; init; } = new();

    [JsonPropertyName("promptCapabilities")]
    public PromptCapabilities PromptCapabilities { get; init; } = new();
}

public record McpCapabilities
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("http")]
    public bool Http { get; init; } = false;

    [JsonPropertyName("sse")]
    public bool Sse { get; init; } = false;
}

public record PromptCapabilities
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("audio")]
    public bool Audio { get; init; } = false;

    [JsonPropertyName("embeddedContext")]
    public bool EmbeddedContext { get; init; } = false;

    [JsonPropertyName("image")]
    public bool Image { get; init; } = false;
}
