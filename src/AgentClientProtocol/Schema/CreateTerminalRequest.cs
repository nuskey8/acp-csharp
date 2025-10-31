using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record CreateTerminalRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("command")]
    public required string Command { get; init; }

    [JsonPropertyName("args")]
    public string[] Args { get; init; } = [];

    [JsonPropertyName("cwd")]
    public string? Cwd { get; init; }

    [JsonPropertyName("env")]
    public EnvVariable[] Env { get; init; } = [];

    [JsonPropertyName("outputByteLimit")]
    public ulong? OutputByteLimit { get; init; }
}
