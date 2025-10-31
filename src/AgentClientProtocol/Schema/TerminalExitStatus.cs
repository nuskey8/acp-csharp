using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record TerminalExitStatus
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("exitCode")]
    public uint? ExitCode { get; init; }

    [JsonPropertyName("signal")]
    public string? Signal { get; init; }
}
