using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(CustomizableJsonStringEnumConverter<StopReason>))]
public enum StopReason
{
    [JsonStringEnumMemberName("end_turn")]
    EndTurn,

    [JsonStringEnumMemberName("max_tokens")]
    MaxTokens,

    [JsonStringEnumMemberName("max_turn_requests")]
    MaxTurnRequests,

    [JsonStringEnumMemberName("refusal")]
    Refusal,

    [JsonStringEnumMemberName("cancelled")]
    Cancelled
}
