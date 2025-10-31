using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(JsonRpcMessageJsonConverter))]
public abstract record JsonRpcMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
}

public record JsonRpcRequest : JsonRpcMessage
{
    [JsonPropertyName("id")]
    public RequestId Id { get; set; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("params")]
    public JsonElement Params { get; init; }
}

public record JsonRpcResponse : JsonRpcMessage
{
    [JsonPropertyName("id")]
    public required RequestId Id { get; init; }

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Result { get; init; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? Error { get; init; }
}

public record JsonRpcNotification : JsonRpcMessage
{
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Params { get; init; }
}

public record JsonRpcError
{
    [JsonPropertyName("code")]
    public required int Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; init; }
}

public sealed class JsonRpcMessageJsonConverter : JsonConverter<JsonRpcMessage>
{
    public override JsonRpcMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("jsonrpc", out var versionProperty))
        {
            throw new JsonException("Missing jsonrpc version");
        }

        if (versionProperty.GetString() != "2.0")
        {
            throw new JsonException("Invalidg jsonrpc version");
        }

        var hasId = root.TryGetProperty("id", out _);
        var hasMethod = root.TryGetProperty("method", out _);

        var rawText = root.GetRawText();

        if (hasId && !hasMethod)
        {
            if (root.TryGetProperty("error", out _) || root.TryGetProperty("result", out _))
            {
                return JsonSerializer.Deserialize(rawText, options.GetTypeInfo<JsonRpcResponse>());
            }

            throw new JsonException("Response must have either result or error");
        }

        if (hasMethod && !hasId)
        {
            return JsonSerializer.Deserialize(rawText, options.GetTypeInfo<JsonRpcNotification>());
        }

        if (hasMethod && hasId)
        {
            return JsonSerializer.Deserialize(rawText, options.GetTypeInfo<JsonRpcRequest>());
        }

        throw new JsonException("Invalid JSON-RPC message format");
    }

    public override void Write(Utf8JsonWriter writer, JsonRpcMessage value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case JsonRpcRequest request:
                JsonSerializer.Serialize(writer, request, options.GetTypeInfo<JsonRpcRequest>());
                break;
            case JsonRpcNotification notification:
                JsonSerializer.Serialize(writer, notification, options.GetTypeInfo<JsonRpcNotification>());
                break;
            case JsonRpcResponse response:
                JsonSerializer.Serialize(writer, response, options.GetTypeInfo<JsonRpcResponse>());
                break;
            default:
                throw new JsonException($"Unknown JSON-RPC message type: {value.GetType()}");
        }
    }
}