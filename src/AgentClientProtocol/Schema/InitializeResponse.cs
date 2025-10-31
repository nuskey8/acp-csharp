using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record InitializeResponse
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("protocolVersion")]
    public required ushort ProtocolVersion { get; init; }

    [JsonPropertyName("agentCapabilities")]
    public AgentCapabilities AgentCapabilities { get; init; } = new();

    [JsonPropertyName("agentInfo")]
    public Implementation? AgentInfo { get; init; }

    [JsonPropertyName("authMethods")]
    public AuthMethod[] AuthMethods { get; init; } = [];
}
