using System.Text.Json;

namespace AgentClientProtocol;

public sealed class ClientSideConnection : IDisposable, IAcpAgent
{
    readonly IAcpClient client;

    readonly CancellationTokenSource cts = new();
    readonly JsonRpcEndpoint endpoint;

    public ClientSideConnection(Func<IAcpAgent, IAcpClient> toClient, TextReader reader, TextWriter writer)
    {
        client = toClient(this);

        endpoint = new(
            _ => new(reader.ReadLine()),
            (s, _) =>
            {
                writer.WriteLine(s);
                return default;
            },
            (s, _) => default
        );

        endpoint.SetRequestHandler(ClientMethods.FsReadTextFile, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.ReadTextFileAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<ReadTextFileRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<ReadTextFileResponse>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.FsWriteTextFile, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.WriteTextFileAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<WriteTextFileRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<WriteTextFileResponse>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.SessionRequestPermission, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.RequestPermissionAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<RequestPermissionRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<RequestPermissionResponse>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.TerminalCreate, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.CreateTerminalAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<CreateTerminalRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<CreateTerminalResponse>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.TerminalKill, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.KillTerminalCommandAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<KillTerminalCommandRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<KillTerminalCommandResponse>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.TerminalOutput, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.TerminalOutputAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<TerminalOutputRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<TerminalOutputRequest>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.TerminalRelease, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.ReleaseTerminalAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<ReleaseTerminalRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<ReleaseTerminalResponse>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.TerminalWaitForExit, async (request, ct) =>
        {
            AcpException.ThrowIfParamIsNull(request.Params);

            var response = await client.WaitForTerminalExitAsync(JsonSerializer.Deserialize(
                request.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<WaitForTerminalExitRequest>())!, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<WaitForTerminalExitResponse>())
            };
        });

        endpoint.SetNotificationHandler(ClientMethods.SessionUpdate, async (notification, ct) =>
        {
            AcpException.ThrowIfParamIsNull(notification.Params);

            var sessionNotification = JsonSerializer.Deserialize(
                notification.Params!.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<SessionNotification>())!;

            await client.SessionNotificationAsync(sessionNotification, ct);
        });

        endpoint.SetDefaultRequestHandler(async (request, ct) =>
        {
            var response = await client.ExtMethodAsync(request.Method, request.Params ?? default, ct);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = response
            };
        });

        endpoint.SetDefaultNotificationHandler(async (notification, ct) =>
        {
            await client.ExtNotificationAsync(notification.Method, notification.Params ?? default, ct);
        });
    }

    async ValueTask<TResponse> RequestAsync<TRequest, TResponse>(string method, TRequest request, CancellationToken cancellationToken)
    {
        var response = await endpoint.SendRequestAsync(new JsonRpcRequest
        {
            Method = method,
            Id = default,
            Params = JsonSerializer.SerializeToElement(request, AcpJsonSerializerContext.Default.Options.GetTypeInfo<TRequest>())
        }, cancellationToken);

        if (response.Error != null)
        {
            throw new AcpException($"{response.Error!.Message}", response.Error.Data, response.Error.Code);
        }

        // HACK: 
        // In a specific version of Gemini-CLI, the `authenticate` method returns a response (`result: null`) 
        // that differs from the expected schema. To accommodate this, we are ignoring the null check. 
        // Since `result` should not be null in any other case, this should generally not be a problem.
        if (response.Result == null)
        {
            return default!;
        }

        return JsonSerializer.Deserialize(response.Result.Value, AcpJsonSerializerContext.Default.Options.GetTypeInfo<TResponse>())!;
    }


    async ValueTask NotificationAsync<TNotification>(string method, TNotification notification, CancellationToken cancellationToken)
    {
        await endpoint.SendMessageAsync(new JsonRpcNotification
        {
            Method = method,
            Params = JsonSerializer.SerializeToElement(notification, AcpJsonSerializerContext.Default.Options.GetTypeInfo<TNotification>())
        }, cancellationToken);
    }

    public ValueTask<InitializeResponse> InitializeAsync(InitializeRequest request, CancellationToken cancellationToken = default)
    {
        return RequestAsync<InitializeRequest, InitializeResponse>(AgentMethods.Initialize, request, cancellationToken);
    }

    public ValueTask<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken = default)
    {
        return RequestAsync<AuthenticateRequest, AuthenticateResponse>(AgentMethods.Authenticate, request, cancellationToken);
    }

    public ValueTask<NewSessionResponse> NewSessionAsync(NewSessionRequest request, CancellationToken cancellationToken = default)
    {
        return RequestAsync<NewSessionRequest, NewSessionResponse>(AgentMethods.SessionNew, request, cancellationToken);
    }

    public ValueTask<PromptResponse> PromptAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        return RequestAsync<PromptRequest, PromptResponse>(AgentMethods.SessionPrompt, request, cancellationToken);
    }

    public ValueTask CancelAsync(CancelNotification notification, CancellationToken cancellationToken = default)
    {
        return NotificationAsync(AgentMethods.SessionCancel, notification, cancellationToken);
    }

    public ValueTask<LoadSessionResponse> LoadSessionAsync(LoadSessionRequest request, CancellationToken cancellationToken = default)
    {
        return RequestAsync<LoadSessionRequest, LoadSessionResponse>(AgentMethods.SessionLoad, request, cancellationToken);
    }

    public ValueTask<SetSessionModeResponse> SetSessionModeAsync(SetSessionModeRequest request, CancellationToken cancellationToken = default)
    {
        return RequestAsync<SetSessionModeRequest, SetSessionModeResponse>(AgentMethods.SessionSetMode, request, cancellationToken);
    }

    public ValueTask<SetSessionModelResponse> SetSessionModelAsync(SetSessionModelRequest request, CancellationToken cancellationToken = default)
    {
        return RequestAsync<SetSessionModelRequest, SetSessionModelResponse>(AgentMethods.SessionSetModel, request, cancellationToken);
    }

    public async ValueTask<JsonElement> ExtMethodAsync(string method, JsonElement request, CancellationToken cancellationToken = default)
    {
        var response = await endpoint.SendRequestAsync(new JsonRpcRequest
        {
            Method = method,
            Id = default,
            Params = request,
        }, cancellationToken);

        if (response.Result == null)
        {
            throw new AcpException($"{response.Error!.Message}", response.Error.Data, response.Error.Code);
        }

        return response.Result.Value;
    }

    public ValueTask ExtNotificationAsync(string method, JsonElement notification, CancellationToken cancellationToken = default)
    {
        // writer.WriteLineAsync(notification.ToString());
        return default;
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }

    public void Open()
    {
        Task.Run(async () => await endpoint.ReadMessagesAsync(cts.Token));
    }
}