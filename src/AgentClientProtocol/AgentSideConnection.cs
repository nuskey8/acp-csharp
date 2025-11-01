using System.Text.Json;

namespace AgentClientProtocol;

public sealed class AgentSideConnection : IDisposable
{
    readonly CancellationTokenSource cts = new();
    readonly JsonRpcEndpoint endpoint;

    public AgentSideConnection(IAcpAgent agent, TextReader reader, TextWriter writer)
    {
        endpoint = new(
            _ => new(reader.ReadLine()),
            (s, _) =>
            {
                writer.WriteLine(s);
                return default;
            },
            (s, _) => default
        );

        endpoint.SetRequestHandler(AgentMethods.Initialize, async (request, ct) =>
        {
            var response = await agent.InitializeAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<InitializeRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<InitializeResponse>())
            };
        });

        endpoint.SetRequestHandler(AgentMethods.Authenticate, async (request, ct) =>
        {
            var response = await agent.AuthenticateAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<AuthenticateRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<AuthenticateResponse>())
            };
        });

        endpoint.SetRequestHandler(AgentMethods.SessionNew, async (request, ct) =>
        {
            var response = await agent.NewSessionAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<NewSessionRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<NewSessionResponse>())
            };
        });

        endpoint.SetRequestHandler(AgentMethods.SessionPrompt, async (request, ct) =>
        {
            var response = await agent.PromptAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<PromptRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<PromptResponse>())
            };
        });

        endpoint.SetRequestHandler(AgentMethods.SessionLoad, async (request, ct) =>
        {
            var response = await agent.LoadSessionAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<LoadSessionRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<LoadSessionResponse>())
            };
        });

        endpoint.SetRequestHandler(AgentMethods.SessionSetMode, async (request, ct) =>
        {
            var response = await agent.SetSessionModeAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModeRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModeResponse>())
            };
        });

        endpoint.SetRequestHandler(AgentMethods.SessionSetModel, async (request, ct) =>
        {
            var response = await agent.SetSessionModelAsync(JsonSerializer.Deserialize(
                request.Params,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModelRequest>())!, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(response, AcpJsonSerializerContext.Default.Options.GetTypeInfo<SetSessionModelResponse>())
            };
        });

        endpoint.SetNotificationHandler(AgentMethods.SessionCancel, async (notification, ct) =>
        {
            if (!notification.Params.HasValue)
            {
                throw new AcpException("Params is null", (int)JsonRpcErrorCode.InvalidParams);
            }

            var cancelNotification = JsonSerializer.Deserialize(
                notification.Params.Value,
                AcpJsonSerializerContext.Default.Options.GetTypeInfo<CancelNotification>())!;
            await agent.CancelAsync(cancelNotification, ct);
        });

        endpoint.SetDefaultRequestHandler(async (request, ct) =>
        {
            var response = await agent.ExtMethodAsync(request.Method, request.Params, ct);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = response
            };
        });

        endpoint.SetDefaultNotificationHandler(async (notification, ct) =>
        {
            await agent.ExtNotificationAsync(notification.Method, notification.Params ?? default, ct);
        });
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