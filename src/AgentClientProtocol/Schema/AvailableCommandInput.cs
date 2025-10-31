using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record AvailableCommandInput
{
    [JsonPropertyName("hint")]
    public required string Hint { get; init; }
}
