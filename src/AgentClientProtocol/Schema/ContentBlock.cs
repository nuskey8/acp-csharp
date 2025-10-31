using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(ContentBlockJsonConverter))]
public abstract record ContentBlock
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    [JsonPropertyName("_meta")]
    public JsonElement? Meta { get; init; }

    [JsonPropertyName("annotations")]
    public Annotations? Annotations { get; init; }
}

public record TextContentBlock : ContentBlock
{
    [JsonPropertyName("type")]
    public override string Type => "text";

    [JsonPropertyName("text")]
    public required string Text { get; init; }
}

public record ImageContentBlock : ContentBlock
{
    [JsonPropertyName("type")]
    public override string Type => "image";

    [JsonPropertyName("data")]
    public required string Data { get; init; }

    [JsonPropertyName("mimeType")]
    public required string MimeType { get; init; }

    [JsonPropertyName("uri")]
    public string? Uri { get; init; }
}

public record AudioContentBlock : ContentBlock
{
    [JsonPropertyName("type")]
    public override string Type => "audio";

    [JsonPropertyName("data")]
    public required string Data { get; init; }

    [JsonPropertyName("mimeType")]
    public required string MimeType { get; init; }
}

public record ResourceLinkContentBlock : ContentBlock
{
    [JsonPropertyName("type")]
    public override string Type => "resource_link";

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; init; }

    [JsonPropertyName("size")]
    public long? Size { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }
}

public record ResourceContentBlock : ContentBlock
{
    [JsonPropertyName("type")]
    public override string Type => "resource";

    [JsonPropertyName("resource")]
    public required EmbeddedResourceResource Resource { get; init; }
}

public class ContentBlockJsonConverter : JsonConverter<ContentBlock>
{
    public override ContentBlock? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeProperty))
        {
            throw new JsonException("Missing 'type' property in ContentBlock");
        }

        var type = typeProperty.GetString();
        return type switch
        {
            "text" => root.Deserialize<TextContentBlock>(options),
            "image" => root.Deserialize<ImageContentBlock>(options),
            "audio" => root.Deserialize<AudioContentBlock>(options),
            "resource_link" => root.Deserialize<ResourceLinkContentBlock>(options),
            "resource" => root.Deserialize<ResourceContentBlock>(options),
            _ => throw new JsonException($"Unknown ContentBlock type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ContentBlock value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}