using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record CreateTerminalResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("terminalId")]
    public required string TerminalId { get; init; }
}
