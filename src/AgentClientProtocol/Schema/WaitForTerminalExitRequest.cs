using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record WaitForTerminalExitRequest
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }

    [JsonPropertyName("terminalId")]
    public required string TerminalId { get; init; }
}
