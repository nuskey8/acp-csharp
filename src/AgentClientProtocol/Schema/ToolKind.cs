using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(CustomizableJsonStringEnumConverter<Role>))]
public enum ToolKind
{
    [JsonStringEnumMemberName("read")]
    Read,

    [JsonStringEnumMemberName("edit")]
    Edit,

    [JsonStringEnumMemberName("delete")]
    Delete,

    [JsonStringEnumMemberName("move")]
    Move,

    [JsonStringEnumMemberName("search")]
    Search,

    [JsonStringEnumMemberName("execute")]
    Execute,

    [JsonStringEnumMemberName("think")]
    Think,

    [JsonStringEnumMemberName("fetch")]
    Fetch,

    [JsonStringEnumMemberName("switch_mode")]
    SwitchMode,

    [JsonStringEnumMemberName("other")]
    Other
}
