using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

public record PlanEntry
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("priority")]
    public required PlanEntryPriority Priority { get; init; }

    [JsonPropertyName("status")]
    public required PlanEntryStatus Status { get; init; }
}


[JsonConverter(typeof(CustomizableJsonStringEnumConverter<Role>))]
public enum PlanEntryPriority
{
    [JsonStringEnumMemberName("high")]
    High,

    [JsonStringEnumMemberName("medium")]
    Medium,

    [JsonStringEnumMemberName("low")]
    Low
}


[JsonConverter(typeof(CustomizableJsonStringEnumConverter<Role>))]
public enum PlanEntryStatus
{
    [JsonStringEnumMemberName("pending")]
    Pending,

    [JsonStringEnumMemberName("in_progress")]
    InProgress,

    [JsonStringEnumMemberName("completed")]
    Completed
}
