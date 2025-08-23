# VirtualRyan A2A (Application-to-Application) Implementation
This document describes the A2A protocol implementation for VirtualRyan, enabling seamless bot-to-bot communication for accessing professional information.

## Overview
The A2A implementation allows other AI agents and bots to programmatically query VirtualRyan for information about:
- Technical skills, programming languages, professional skills, etc.
- Work experience and project history
- Educational background
- Professional qualifications
- Career objectives

## Architecture
### Server Components (VirtualRyan.Server)
- **A2AService**: Contains core business logic for processing A2A requests using `MessageSendParams`, `Message`, and `TextPart` models for structured agent-to-agent messaging

## Client Quick Start Example
The following example demonstrates how to discover the agent, create a client, and send a message using the updated A2A protocol:

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
- **MaxRequestsPerMinute**: Maximum requests allowed per minute
- **MaxConcurrentRequests**: Maximum concurrent requests allowed

### Server Configuration (appsettings.json)
```json
{
  "A2A": {
    "Agent": {
      "Name": "VirtualRyan",
      "Version": "1.0.0",
      "Description": "Virtual assistant for Ryan Russon",
      "IconUrl": "https://avatars.githubusercontent.com/u/653188?v=4",
      "MaxRequestsPerMinute": 60,
      "MaxConcurrentRequests": 10
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
POST {{VirtualRyan.Server_HostAddress}}/ask
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
