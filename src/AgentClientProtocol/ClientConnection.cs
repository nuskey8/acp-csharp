using System.Text.Json;

namespace AgentClientProtocol;

public sealed class ClientConnection : IDisposable, IAcpAgent
{
    readonly IAcpClient client;

    // TODO: optimize I/O
    readonly CancellationTokenSource cts = new();
    readonly JsonRpcEndpoint endpoint;

    public ClientConnection(Func<IAcpAgent, IAcpClient> toClient, TextReader reader, TextWriter writer)
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
            var response = await client.ReadTextFileAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<ReadTextFileRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<ReadTextFileResponse>())
            };
        });

        endpoint.SetRequestHandler(ClientMethods.FsWriteTextFile, async (request, ct) =>
        {
            var response = await client.WriteTextFileAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<WriteTextFileRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<WriteTextFileResponse>())
            };
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

        if (response.Result == null)
        {
            throw new AcpException($"{response.Error!.Message}: {response.Error.Data}", response.Error.Code);
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
            throw new AcpException($"{response.Error!.Message}: {response.Error.Data}", response.Error.Code);
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