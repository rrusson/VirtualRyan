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
- **A2AService**: Contains the core business logic for processing A2A requests using `MessageSendParams`, `Message`, and `TextPart` models for structured agent-to-agent messaging

## Client Quick Start Example
The following example demonstrates how to discover the agent, create a client, and send a message using the [A2A.Net library](https://github.com/a2aproject/a2a-dotnet):

```csharp
using A2A;

// Discover agent and create client
var cardResolver = new A2ACardResolver(new Uri("http://localhost:5074/"));
var agentCard = await cardResolver.GetAgentCardAsync();
var client = new A2AClient(new Uri(agentCard.Url));

// Send message
var response = await client.SendMessageAsync(new MessageSendParams
{
    Message = new Message
    {
        Role = MessageRole.User,
        Parts = [new TextPart { Text = "How long was the last job?" }]
    }
});
```

## Configuration
### Client Configuration Options
- **Agent:Name**: Name identifying your bot/agent
- **Agent:Version**: Version of your agent
- **Agent:Description**: Brief description of your agent's purpose
- **IconUrl**: URL to an icon representing your agent
- **Provider**: Name of the provider/organization
- **ProviderUrl**: URL for the provider/organization
- **DocumentationUrl**: URL for documentation
- **MaxRequestsPerMinute**: Maximum requests allowed per minute
- **MaxConcurrentRequests**: Maximum concurrent requests allowed

### A2A Server Configuration (in appsettings.json)
```json
{
  "A2A": {
    "Agent": {
      "Name": "VirtualRyan",
      "Version": "1.0.0",
      "Description": "Bot-to-bot access to Ryan Russon's professional information.",
      "IconUrl": "https://avatars.githubusercontent.com/u/653188?v=4",
      "Provider": "r.russon consulting",
      "ProviderUrl": "https://ai.rrusson.com",
      "DocumentationUrl": "https://github.com/rrusson/VirtualRyan/blob/main/A2A-IMPLEMENTATION-GUIDE.md",
      "MaxRequestsPerMinute": 30,
      "MaxRequestsPerDay": 100
    },
    "Features": {
      "EnableDetailedLogging": true,
      "EnableMetrics": true,
      "EnableCaching": false
    }
  }
}
```

## Testing
### HTTP Client Tests
Use the provided `VirtualRyan.Server.http` file to test endpoints:

```http
# Agent info
GET {{VirtualRyan.Server_HostAddress}}/.well-known/agent.json

# Simple question
POST {{VirtualRyan.Server_HostAddress}}/a2a
Content-Type: application/json
Accept: application/json
{
  "jsonrpc": "2.0",
  "method": "message/send",
  "id": "test-001",
  "params": {
    "message": {
      "role": "user",
      "messageId": "test-msg-001",
      "contextId": "test-context",
      "parts": [
        {
          "kind":"text",
          "text": "What skills did you use at your last job?"
        }
      ]
    }
  }
}
```
