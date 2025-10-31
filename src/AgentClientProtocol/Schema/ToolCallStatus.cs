using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(CustomizableJsonStringEnumConverter<Role>))]
public enum ToolCallStatus
{
    [JsonStringEnumMemberName("pending")]
    Pending,

    [JsonStringEnumMemberName("in_progress")]
    InProgress,

    [JsonStringEnumMemberName("completed")]
    Completed,

    [JsonStringEnumMemberName("failed")]
    Failed
}
