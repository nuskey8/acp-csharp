using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(CustomizableJsonStringEnumConverter<Role>))]
public enum Role
{
    [JsonStringEnumMemberName("assistant")]
    Assistant,

    [JsonStringEnumMemberName("user")]
    User
}
