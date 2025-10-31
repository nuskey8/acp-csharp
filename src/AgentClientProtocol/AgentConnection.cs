using System.Text.Json;

namespace AgentClientProtocol;

public sealed class AgentConnection(IAcpAgent agent, TextReader reader, TextWriter writer) : IDisposable
{
    readonly CancellationTokenSource cts = new();

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }

    public void Open()
    {
        Task.Run(ReadMessagesAsync, cts.Token);
    }

    async Task ReadMessagesAsync()
    {
        await Task.Yield();

        while (true)
        {
            cts.Token.ThrowIfCancellationRequested();

            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var trimmedLineSpan = line.AsSpan().Trim();
            if (trimmedLineSpan.Length < 2 || trimmedLineSpan[0] != '{' || trimmedLineSpan[^1] != '}') continue; // skip non-json input

            var message = JsonSerializer.Deserialize(line, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()!);

            switch (message)
            {
                case JsonRpcRequest request:
                    try
                    {
                        JsonRpcResponse rpcResponse;
                        switch (request.Method)
                        {
                            case AgentMethods.Initialize:
                                {
                                    var response = await agent.InitializeAsync(
                                        JsonSerializer.Deserialize(request.Params!, AcpJsonSerializerContext.Default.Options.GetTypeInfo<InitializeRequest>())!,
                                        cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<InitializeResponse>()!),
                                    };
                                }
                                break;
                            case AgentMethods.Authenticate:
                                {
                                    var response = await agent.AuthenticateAsync(
                                        JsonSerializer.Deserialize(request.Params!, AcpJsonSerializerContext.Default.Options.GetTypeInfo<AuthenticateRequest>())!,
                                        cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<AuthenticateResponse>()!),
                                    };
                                }
                                break;
                            case AgentMethods.SessionNew:
                                {
                                    var response = await agent.NewSessionAsync(
                                        JsonSerializer.Deserialize(request.Params!, AcpJsonSerializerContext.Default.Options.GetTypeInfo<NewSessionRequest>())!,
                                        cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<NewSessionResponse>()!),
                                    };
                                }
                                break;
                            case AgentMethods.SessionLoad:
                                {
                                    var response = await agent.LoadSessionAsync(
                                        JsonSerializer.Deserialize(request.Params!, AcpJsonSerializerContext.Default.Options.GetTypeInfo<LoadSessionRequest>())!,
                                        cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<LoadSessionResponse>()!),
                                    };
                                }
                                break;
                            case AgentMethods.SessionPrompt:
                                {
                                    var response = await agent.PromptAsync(
                                        JsonSerializer.Deserialize(request.Params!, AcpJsonSerializerContext.Default.Options.GetTypeInfo<PromptRequest>())!,
                                        cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<PromptResponse>()!),
                                    };
                                }
                                break;
                            case AgentMethods.SessionSetMode:
                                {
                                    var response = await agent.SetSessionModeAsync(
                                        JsonSerializer.Deserialize(request.Params!, AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModeRequest>())!,
                                        cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModeResponse>()!),
                                    };
                                }
                                break;
                            case AgentMethods.SessionSetModel:
                                {
                                    var response = await agent.SetSessionModelAsync(
                                        JsonSerializer.Deserialize(request.Params!, AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModelRequest>())!,
                                        cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModelResponse>()!),
                                    };
                                }
                                break;
                            default:
                                {
                                    var response = await agent.ExtMethodAsync(request.Method, request.Params, cts.Token);
                                    rpcResponse = new()
                                    {
                                        Id = request.Id,
                                        Result = response,
                                    };
                                }
                                break;
                        }

                        writer.WriteLine(JsonSerializer.Serialize(rpcResponse, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcResponse>()));
                    }
                    catch (OperationCanceledException) when (cts.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = new JsonRpcResponse
                        {
                            Id = request.Id,
                            Error = new JsonRpcError
                            {
                                Code = -32000,
                                Message = ex.Message,
                                Data = ex.ToString()
                            }
                        };
                        writer.WriteLine(JsonSerializer.Serialize(errorResponse, AcpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcResponse>()));
                    }
                    break;
                case JsonRpcNotification notification:
                    try
                    {
                        switch (notification.Method)
                        {
                            case AgentMethods.SessionCancel:
                                await agent.CancelAsync(
                                    JsonSerializer.Deserialize(notification.Method, AcpJsonSerializerContext.Default.Options.GetTypeInfo<CancelNotification>())!,
                                    cts.Token);
                                break;
                        }
                    }
                    catch (OperationCanceledException) when (cts.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                    break;
                default:
                    throw new AcpException($"Invalid request type: {message?.GetType().Name}", -32000);
            }
        }
    }
}