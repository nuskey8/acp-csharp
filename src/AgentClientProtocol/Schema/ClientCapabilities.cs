using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record ClientCapabilities
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("fs")]
    public FileSystemCapability Fs { get; init; } = new();

    [JsonPropertyName("terminal")]
    public bool Terminal { get; init; } = false;
}

public record FileSystemCapability
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("readTextFile")]
    public bool ReadTextFile { get; init; } = false;

    [JsonPropertyName("writeTextFile")]
    public bool WriteTextFile { get; init; } = false;
}
