using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(SessionUpdateJsonConverter))]
public abstract record SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public abstract string Update { get; }
}

public class SessionUpdateJsonConverter : JsonConverter<SessionUpdate>
{
    public override SessionUpdate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("sessionUpdate", out var sessionUpdateProperty))
        {
            throw new JsonException("Missing 'sessionUpdate' property in SessionUpdate");
        }

        var type = sessionUpdateProperty.GetString();
        return type switch
        {
            "user_message_chunk" => root.Deserialize<UserMessageChunkSessionUpdate>(options),
            "agent_message_chunk" => root.Deserialize<AgentMessageChunkSessionUpdate>(options),
            "agent_thought_chunk" => root.Deserialize<AgentThoughtChunkSessionUpdate>(options),
            "tool_call" => root.Deserialize<ToolCallSessionUpdate>(options),
            "tool_call_update" => root.Deserialize<ToolCallUpdateSessionUpdate>(options),
            "plan" => root.Deserialize<PlanSessionUpdate>(options),
            "available_commands_update" => root.Deserialize<AvailableCommandsUpdateSessionUpdate>(options),
            "current_mode_update" => root.Deserialize<CurrentModeUpdateSessionUpdate>(options),
            _ => throw new JsonException($"Unknown SessionUpdate type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, SessionUpdate value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

public record UserMessageChunkSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "user_message_chunk";

    [JsonPropertyName("content")]
    public required ContentBlock Content { get; init; }
}

public record AgentMessageChunkSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "agent_message_chunk";

    [JsonPropertyName("content")]
    public required ContentBlock Content { get; init; }
}

public record AgentThoughtChunkSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "agent_thought_chunk";

    [JsonPropertyName("content")]
    public required ContentBlock Content { get; init; }
}

public record ToolCallSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "tool_call";

    [JsonPropertyName("toolCallId")]
    public required string ToolCallId { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("content")]
    public ToolCallContent[] Content { get; init; } = [];

    [JsonPropertyName("kind")]
    public ToolKind Kind { get; init; }

    [JsonPropertyName("locations")]
    public ToolCallLocation[] Locations { get; init; } = [];

    [JsonPropertyName("rawInput")]
    public JsonElement? RawInput { get; init; }

    [JsonPropertyName("rawOutput")]
    public JsonElement? RawOutput { get; init; }

    [JsonPropertyName("status")]
    public ToolCallStatus Status { get; init; }
}

public record ToolCallUpdateSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "tool_call_update";

    [JsonPropertyName("toolCallId")]
    public required string ToolCallId { get; init; }

    [JsonPropertyName("content")]
    public ToolCallContent[]? Content { get; init; }

    [JsonPropertyName("kind")]
    public ToolKind? Kind { get; init; }

    [JsonPropertyName("locations")]
    public ToolCallLocation[]? Locations { get; init; }

    [JsonPropertyName("rawInput")]
    public JsonElement? RawInput { get; init; }

    [JsonPropertyName("rawOutput")]
    public JsonElement? RawOutput { get; init; }

    [JsonPropertyName("status")]
    public ToolCallStatus? Status { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }
}

public record PlanSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "plan";

    [JsonPropertyName("entries")]
    public required PlanEntry[] Entries { get; init; }
}

public record AvailableCommandsUpdateSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "available_commands_update";

    [JsonPropertyName("availableCommands")]
    public required AvailableCommand[] AvailableCommands { get; init; }
}

public record CurrentModeUpdateSessionUpdate : SessionUpdate
{
    [JsonPropertyName("sessionUpdate")]
    public override string Update => "current_mode_update";

    [JsonPropertyName("currentModeId")]
    public required string CurrentModeId { get; init; }
}
