using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record TerminalOutputResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("output")]
    public required string Output { get; init; }

    [JsonPropertyName("truncated")]
    public required bool Truncated { get; init; }

    [JsonPropertyName("exitStatus")]
    public TerminalExitStatus? ExitStatus { get; init; }
}
