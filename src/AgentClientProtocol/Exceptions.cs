namespace AgentClientProtocol;

public class AcpException(string? message, int code, Exception? innerException = null) : Exception(message, innerException)
{
    public int Code { get; } = code;
}