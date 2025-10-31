using System.Diagnostics;
using System.Text.Json;
using AgentClientProtocol;

var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "gemini",
        Arguments = "--experimental-acp",
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardInput = true,
    },
};

try
{
    if (!process.Start())
    {
        throw new Exception("Failed to start agent proccess");
    }

    var client = new ExampleClient();
    var connection = new ClientConnection(_ => client, process.StandardOutput, process.StandardInput);

    connection.Open();

    var initResult = await connection.InitializeAsync(new InitializeRequest
    {
        ProtocolVersion = 1,
        ClientCapabilities = new ClientCapabilities
        {
            Fs = new FileSystemCapability
            {
                ReadTextFile = true,
                WriteTextFile = true
            }
        }
    });

    Console.WriteLine($"✅ Connected to agent (protocol v{initResult.ProtocolVersion})");

    var sessionResult = await connection.NewSessionAsync(new NewSessionRequest
    {
        Cwd = Directory.GetCurrentDirectory(),
        McpServers = []
    });

    Console.WriteLine($"📝 Created session: {sessionResult.SessionId}");
    Console.WriteLine("💬 User: Hello, agent!\n");
    Console.Write(" ");

    var promptResult = await connection.PromptAsync(new PromptRequest
    {
        SessionId = sessionResult.SessionId,
        Prompt = [new TextContentBlock { Text = "Hello, agent!" }]
    });

    Console.WriteLine($"\n\n✅ Agent completed with: {promptResult.StopReason}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[Client] Error: {ex}");
}
finally
{
    process.Kill();
}

class ExampleClient : IAcpClient
{
    public async ValueTask<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request, CancellationToken cancellationToken = default)
    {
        var toolCallElement = (JsonElement)request.ToolCall;
        var title = toolCallElement.GetProperty("title").GetString() ?? "Unknown";
        Console.WriteLine($"\n🔐 Permission requested: {title}");

        Console.WriteLine("\nOptions:");
        for (int i = 0; i < request.Options.Length; i++)
        {
            var option = request.Options[i];
            Console.WriteLine($"   {i + 1}. {option.Name} ({option.Kind})");
        }

        while (true)
        {
            Console.Write("\nChoose an option: ");
            var answer = Console.ReadLine()?.Trim();
            if (int.TryParse(answer, out var optionIndex) && optionIndex >= 1 && optionIndex <= request.Options.Length)
            {
                return new RequestPermissionResponse
                {
                    Outcome = new SelectedRequestPermissionOutcome { OptionId = request.Options[optionIndex - 1].OptionId }
                };
            }
            else
            {
                Console.WriteLine("Invalid option. Please try again.");
            }
        }
    }

    public async ValueTask SessionNotificationAsync(SessionNotification notification, CancellationToken cancellationToken = default)
    {
        var update = notification.Update;
        switch (update)
        {
            case AgentMessageChunkSessionUpdate agentMessage:
                if (agentMessage.Content is TextContentBlock text)
                {
                    Console.Write(text.Text);
                }
                else
                {
                    Console.WriteLine($"[{agentMessage.Content.GetType().Name}]");
                }
                break;
            case ToolCallSessionUpdate toolCall:
                Console.WriteLine($"\n🔧 {toolCall.Title} ({toolCall.Status})");
                break;
            case ToolCallUpdateSessionUpdate toolCallUpdate:
                Console.WriteLine($"\n🔧 Tool call `{toolCallUpdate.ToolCallId}` updated: {toolCallUpdate.Status}\n");
                break;
            case PlanSessionUpdate:
            case AgentThoughtChunkSessionUpdate:
            case UserMessageChunkSessionUpdate:
                Console.WriteLine($"[{update.GetType().Name}]");
                break;
            default:
                break;
        }
    }

    public async ValueTask<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
    {
        Console.Error.WriteLine($"[Client] Write text file called with: {JsonSerializer.Serialize(request)}");
        return new WriteTextFileResponse();
    }

    public async ValueTask<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
    {
        Console.Error.WriteLine($"[Client] Read text file called with: {JsonSerializer.Serialize(request)}");
        return new ReadTextFileResponse { Content = "Mock file content" };
    }

    public ValueTask<CreateTerminalResponse> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<TerminalOutputRequest> TerminalOutputAsync(TerminalOutputRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<ReleaseTerminalResponse> ReleaseTerminalAsync(ReleaseTerminalRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<WaitForTerminalExitResponse> WaitForTerminalExitAsync(WaitForTerminalExitRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<KillTerminalCommandResponse> KillTerminalCommandAsync(KillTerminalCommandRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<JsonElement> ExtMethodAsync(string method, JsonElement request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask ExtNotificationAsync(string method, JsonElement notification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}