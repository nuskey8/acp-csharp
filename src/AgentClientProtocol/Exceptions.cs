using System.Collections;
using System.Text.Json;

namespace AgentClientProtocol;

public class AcpException(string? message, JsonElement? data, int code, Exception? innerException = null) : Exception(message, innerException)
{
    public JsonElement? ErrorData { get; } = data;
    public int Code { get; } = code;

    internal static void ThrowIfParamIsNull(in JsonElement? param)
    {
        if (!param.HasValue)
        {
            throw new AcpException("Params is null", null, (int)JsonRpcErrorCode.InvalidParams);
        }
    }
}