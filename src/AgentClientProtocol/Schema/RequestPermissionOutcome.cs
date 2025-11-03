using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentClientProtocol;

[JsonConverter(typeof(RequestPermissionOutcomeJsonConverter))]
public abstract record RequestPermissionOutcome
{
    [JsonPropertyName("outcome")]
    public abstract string Outcome { get; }
}

public record CancelledRequestPermissionOutcome : RequestPermissionOutcome
{
    [JsonPropertyName("outcome")]
    public override string Outcome => "cancelled";
}

public record SelectedRequestPermissionOutcome : RequestPermissionOutcome
{
    [JsonPropertyName("outcome")]
    public override string Outcome => "selected";

    [JsonPropertyName("optionId")]
    public required string OptionId { get; init; }
}

public class RequestPermissionOutcomeJsonConverter : JsonConverter<RequestPermissionOutcome>
{
    public override RequestPermissionOutcome? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("RequestPermissionOutcome must be a JSON object");
        }

        if (root.TryGetProperty("optionId", out _))
        {
            return root.Deserialize<SelectedRequestPermissionOutcome>(options);
        }
        else
        {
            return root.Deserialize<CancelledRequestPermissionOutcome>(options);
        }
    }

    public override void Write(Utf8JsonWriter writer, RequestPermissionOutcome value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}