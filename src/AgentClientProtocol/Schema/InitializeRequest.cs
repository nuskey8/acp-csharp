using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record InitializeRequest
{
    [JsonPropertyName("protocolVersion")]
    public required ushort ProtocolVersion { get; init; }

    [JsonPropertyName("_meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("clientCapabilities")]
    public ClientCapabilities ClientCapabilities { get; init; } = new();

    [JsonPropertyName("clientInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Implementation? ClientInfo { get; init; }
}
