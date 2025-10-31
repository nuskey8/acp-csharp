using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(EmbeddedResourceResourceJsonConverter))]
public abstract record EmbeddedResourceResource;

public record TextResourceContents : EmbeddedResourceResource
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; init; }
}

public record BlobResourceContents : EmbeddedResourceResource
{
    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("blob")]
    public required string Blob { get; init; }

    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; init; }
}

public class EmbeddedResourceResourceJsonConverter : JsonConverter<EmbeddedResourceResource>
{
    public override EmbeddedResourceResource? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("text", out _))
        {
            return root.Deserialize<TextResourceContents>(options);
        }
        else if (root.TryGetProperty("blob", out _))
        {
            return root.Deserialize<BlobResourceContents>(options);
        }
        else
        {
            throw new JsonException("Unknown EmbeddedResourceResource type - missing 'text' or 'blob' property");
        }
    }

    public override void Write(Utf8JsonWriter writer, EmbeddedResourceResource value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}