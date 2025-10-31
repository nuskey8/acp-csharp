using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(McpServerJsonConverter))]
public abstract record McpServer
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("type")]
    public abstract string Type { get; }
}

public record HttpMcpServer : McpServer
{
    [JsonPropertyName("type")]
    public override string Type => "http";

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("headers")]
    public required HttpHeader[] Headers { get; init; }
}

public record SseMcpServer : McpServer
{
    [JsonPropertyName("type")]
    public override string Type => "sse";

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("headers")]
    public required HttpHeader[] Headers { get; init; }
}

public record StdioMcpServer : McpServer
{
    [JsonPropertyName("type")]
    public override string Type => "stdio";

    [JsonPropertyName("command")]
    public required string Command { get; init; }

    [JsonPropertyName("args")]
    public required string[] Args { get; init; }

    [JsonPropertyName("env")]
    public required EnvVariable[] Env { get; init; }
}

public class McpServerJsonConverter : JsonConverter<McpServer>
{
    public override McpServer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeProperty))
        {
            throw new JsonException("Missing 'type' property in McpServer");
        }

        var type = typeProperty.GetString();
        return type switch
        {
            "http" => root.Deserialize<HttpMcpServer>(options),
            "sse" => root.Deserialize<SseMcpServer>(options),
            "stdio" => root.Deserialize<StdioMcpServer>(options),
            _ => throw new JsonException($"Unknown McpServer type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, McpServer value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}