# AgentClientProtocol for C#
 Unofficial C# SDK for ACP (Agent Client Protocol) clients and agents

[![NuGet](https://img.shields.io/nuget/v/AgentClientProtocol.svg)](https://www.nuget.org/packages/AgentClientProtocol)
[![Releases](https://img.shields.io/github/release/nuskey8/acp-csharp.svg)](https://github.com/nuskey8/acp-csharp/releases)
[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

## What's Agent Client Protocol?

Agent Client Protocol is a protocol proposed by Zed to standardize communication between code editors/IDEs and coding agents.

> ACP solves this by providing a standardized protocol for agent-editor communication, similar to how the Language Server Protocol (LSP) standardized language server integration.

Please refer to [the official documentation](https://agentclientprotocol.com/) for details.

## Installation

### .NET CLI

```ps1
dotnet add package AgentClientProtocol
```

### Package Manager

```ps1
Install-Package AgentClientProtocol
```

## Quick Start

### Client

```cs
class ExampleClient : IAcpClient { ... }
```

```cs
var client = new ExampleClient();

using var conn = new ClientSideConnection( _ => client, reader, writer);

conn.Open();

var initResult = await conn.InitializeAsync(new InitializeRequest
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

Console.WriteLine($"Connected to agent (protocol v{initResult.ProtocolVersion})");
```

### Agent

```cs
class ExampleClient : IAcpAgent { ... }
```

```cs
var agent = new ExampleAgent();
using var conn = new AgentSideConnection(agent, reader, writer);
conn.Open();

await Task.Delay(-1);
```

## License

This library is under the [MIT License](LICENSE).