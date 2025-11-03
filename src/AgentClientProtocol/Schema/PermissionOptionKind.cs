using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(CustomizableJsonStringEnumConverter<PermissionOptionKind>))]
public enum PermissionOptionKind
{
    [JsonStringEnumMemberName("allow_once")]
    AllowOnce,

    [JsonStringEnumMemberName("allow_always")]
    AllowAlways,

    [JsonStringEnumMemberName("reject_once")]
    RejectOnce,

    [JsonStringEnumMemberName("reject_always")]
    RejectAlways
}
