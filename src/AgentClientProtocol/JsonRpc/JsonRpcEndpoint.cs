using System.Collections.Concurrent;
using System.Text.Json;

namespace AgentClientProtocol;

internal enum JsonRpcErrorCode
{
    ParseError = -32700,
    InvalidRequest = -32600,
    MethodNotFound = -32601,
    InvalidParams = -32602,
    InternalError = -32603
}

internal sealed class JsonRpcEndpoint(Func<CancellationToken, ValueTask<string?>> readFunc, Func<string, CancellationToken, ValueTask> writeFunc, Func<string, CancellationToken, ValueTask> errorWriteFunc)
{
    readonly ConcurrentDictionary<RequestId, TaskCompletionSource<JsonRpcResponse>> pendingRequests = new();
    readonly ConcurrentDictionary<string, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>>> requestHandlers = new();
    readonly ConcurrentDictionary<string, Func<JsonRpcNotification, CancellationToken, ValueTask>> notificationHandlers = new();
    Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>>? defaultRequestHandler;
    Func<JsonRpcNotification, CancellationToken, ValueTask>? defaultNotificationHandler;
    int nextRequestId = 0;

    public void SetRequestHandler(string method, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler)
    {
        requestHandlers.TryAdd(method, handler);
    }

    public void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
    {
        notificationHandlers.TryAdd(method, handler);
    }

    public void SetDefaultRequestHandler(Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler)
    {
        defaultRequestHandler = handler;
    }

    public void SetDefaultNotificationHandler(Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
    {
        defaultNotificationHandler = handler;
    }

    public async Task ReadMessagesAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var line = await readFunc(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(line)) continue;

                var trimmedLineSpan = line.AsSpan().Trim();
                if (trimmedLineSpan.Length < 2 || trimmedLineSpan[0] != '{' || trimmedLineSpan[^1] != '}') continue; // skip non-json input

                var message = JsonSerializer.Deserialize(line, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()!);

                switch (message)
                {
                    case JsonRpcRequest request:
                        try
                        {
                            if (requestHandlers.TryGetValue(request.Method, out var requestHandler))
                            {
                                var response = await requestHandler(request, cancellationToken);
                                await writeFunc(JsonSerializer.Serialize(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()), cancellationToken);
                            }
                            else if (defaultRequestHandler != null)
                            {
                                var response = await defaultRequestHandler(request, cancellationToken);
                                await writeFunc(JsonSerializer.Serialize(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()), cancellationToken);
                            }
                            else
                            {
                                await writeFunc(JsonSerializer.Serialize(new JsonRpcResponse
                                {
                                    Id = request.Id,
                                    Error = new()
                                    {
                                        Code = (int)JsonRpcErrorCode.MethodNotFound,
                                        Message = $"Method '{request.Method}' is not available",
                                    }
                                }, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()), cancellationToken);
                            }
                        }
                        catch (NotImplementedException)
                        {
                            await writeFunc(JsonSerializer.Serialize(new JsonRpcResponse
                            {
                                Id = request.Id,
                                Error = new()
                                {
                                    Code = (int)JsonRpcErrorCode.MethodNotFound,
                                    Message = $"Method '{request.Method}' is not available",
                                }
                            }, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()), cancellationToken);
                        }
                        catch (AcpException acpException)
                        {
                            await writeFunc(JsonSerializer.Serialize(new JsonRpcResponse
                            {
                                Id = request.Id,
                                Error = new()
                                {
                                    Code = acpException.Code,
                                    Data = acpException.ErrorData,
                                    Message = acpException.Message,
                                }
                            }, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()), cancellationToken);
                        }
                        break;
                    case JsonRpcResponse response:
                        {
                            if (pendingRequests.TryRemove(response.Id, out var tcs))
                            {
                                tcs.TrySetResult(response);
                            }
                        }
                        break;
                    case JsonRpcNotification notification:
                        if (notificationHandlers.TryGetValue(notification.Method, out var notificationHandler))
                        {
                            await notificationHandler(notification, cancellationToken);
                        }
                        else if (defaultNotificationHandler != null)
                        {
                            await defaultNotificationHandler(notification, cancellationToken);
                        }
                        break;
                    default:
                        throw new AcpException($"Invalid response type: {message?.GetType().Name}", null, (int)JsonRpcErrorCode.InternalError);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                await errorWriteFunc(ex.ToString(), cancellationToken);
            }
        }
    }

    public async ValueTask SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        if (message is JsonRpcRequest request && !request.Id.IsValid)
        {
            request.Id = Interlocked.Increment(ref nextRequestId);
        }

        var json = JsonSerializer.Serialize(message, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>());
        await writeFunc(json, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<JsonRpcResponse> SendRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.Id.IsValid) request.Id = Interlocked.Increment(ref nextRequestId);

        var json = JsonSerializer.Serialize(request, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcRequest>());

        var tcs = new TaskCompletionSource<JsonRpcResponse>();
        pendingRequests.TryAdd(request.Id, tcs);

        await writeFunc(json, cancellationToken).ConfigureAwait(false);

        return await tcs.Task;
    }
}