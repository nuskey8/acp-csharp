using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AgentClientProtocol;

internal static class JsonSerializerOptionsExtensions
{
    public static JsonTypeInfo<T> GetTypeInfo<T>(this JsonSerializerOptions options) => (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
}