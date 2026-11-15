# VirtualRyan A2A (Agent-to-Agent) Implementation
This document describes the A2A protocol implementation for VirtualRyan, enabling seamless bot-to-bot communication for accessing professional information. (Perhaps one day recruiting agents will find humans and other agents with required skills this way?)

## Overview
The A2A implementation allows other AI agents and bots to programmatically query VirtualRyan for information about:
- Technical skills, programming languages, professional skills, etc.
- Work experience and employment history
- Educational attainment
- Professional qualifications
- Career objectives
- Personal details

## Architecture
### Server Components (VirtualRyan.Server)
- **A2AService**: Processes incoming messages using the configured `Llm:Endpoint`, `Llm:Model`, and `Llm:ApiKey`, the same settings as the chat endpoint. Supply the API key through user secrets or an environment variable such as `Llm__ApiKey`.
- The incoming HTTP Host identifies this server, not the calling agent. The service does not fetch caller cards or infer an authenticated caller identity from request headers. Caller identification would require a separately defined, authenticated trust mechanism.
- An LLM failure is logged and returned as a failed A2A task. Request cancellation is propagated to the SDK.
- The agent card advertises A2A 1.0 JSON-RPC at `/a2a` and HTTP+JSON under `/a2a`, including `/a2a/message:send`. Requests specifying other `A2A-Version` values are rejected; omitting the header uses 1.0.
- Both bindings support SSE streaming. The answer is delivered as one complete message after the LLM finishes, not as incremental tokens.
- The card is unsigned. Signatures are optional; signing requires a securely managed signing key and a distribution mechanism clients can use to verify it.

## Client Quick Start Example
The following example uses [A2A.Net](https://github.com/a2aproject/a2a-dotnet) 1.0.0-preview2. For local testing, set `A2A:Agent:Url` to the server's local base URL so the discovered card advertises the local endpoint.

```csharp
using A2A;

var cardResolver = new A2ACardResolver(new Uri("http://localhost:5074/"));
var agentCard = await cardResolver.GetAgentCardAsync().ConfigureAwait(false);
var jsonRpc = agentCard.SupportedInterfaces.First(i => i.ProtocolBinding == ProtocolBindingNames.JsonRpc);
using var httpClient = new HttpClient();
var client = new A2AClient(new Uri(jsonRpc.Url), httpClient);

var response = await client.SendMessageAsync(new SendMessageRequest
{
    Message = new Message
    {
        Role = Role.User,
        MessageId = Guid.NewGuid().ToString("N"),
        Parts = [Part.FromText("How long was the last job?")]
    }
}).ConfigureAwait(false);
```

## Configuration
Set `A2A:Agent:Url` to the externally reachable base URL. The `MaxRequestsPerMinute` and `MaxRequestsPerDay` settings limit requests per client IP. Configure `Llm:Endpoint` and `Llm:Model` for the provider, and store any required `Llm:ApiKey` outside the configuration file.

### A2A Server Configuration (in appsettings.json)
```json
{
  "A2A": {
    "Agent": {
      "Url": "https://ai.rrusson.com",
      "Name": "VirtualRyan",
      "Version": "1.0.0",
      "Description": "Bot-to-bot access to Ryan Russon's professional information.",
      "IconUrl": "https://avatars.githubusercontent.com/u/653188?v=4",
      "Provider": "r.russon consulting",
      "ProviderUrl": "https://ai.rrusson.com",
      "DocumentationUrl": "https://github.com/rrusson/VirtualRyan/blob/main/A2A-IMPLEMENTATION-GUIDE.md",
      "MaxRequestsPerMinute": 30,
      "MaxRequestsPerDay": 100
    }
  }
}
```

## Testing
### HTTP Client Tests
Use the provided `VirtualRyan.Server.http` file to test endpoints:

```http
# Agent info
GET {{VirtualRyan.Server_HostAddress}}/.well-known/agent-card.json

# Simple question
POST {{VirtualRyan.Server_HostAddress}}/a2a
Content-Type: application/json
Accept: application/json
{
  "jsonrpc": "2.0",
  "method": "SendMessage",
  "id": "test-001",
  "params": {
    "message": {
      "role": "ROLE_USER",
      "messageId": "test-msg-001",
      "contextId": "test-context",
      "parts": [
        {
          "text": "What skills did you use at your last job?"
        }
      ]
    }
  }
}
```
