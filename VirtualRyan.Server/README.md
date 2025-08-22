# VirtualRyan.Server A2A API

## A2A (Agent-to-Agent) API

The A2A API allows other bots or applications to call the chatbot directly via HTTP.

### Endpoint

- **POST** `/api/a2a/ask`
  - Request body: `{ "question": "<your question>" }`
  - Response: `{ "answer": "<chatbot response>" }`

### Throttling
- **2 requests per second** and **50 requests per hour** per IP address.
- Exceeding these limits returns HTTP 429 (Too Many Requests).

### Authentication & Authorization
- By default, authentication is **not required** (`RequireAuth: false` in config).
- If enabled, provide an API key in the `X-API-Key` header. Only requests with valid API keys (see config) are accepted.

### Configuration (appsettings.json)
```json
"A2ASettings": {
  "RequireAuth": false,
  "AllowedClientIds": [ "sample-client-id" ],
  "ApiKeys": [ "sample-api-key" ]
}
```

- Set `RequireAuth` to `true` to enforce API key authentication.
- Add allowed client IDs and API keys as needed.

### Example Request
```http
POST /api/a2a/ask HTTP/1.1
Host: localhost:5074
Content-Type: application/json
X-API-Key: sample-api-key

{
  "question": "What is your name?"
}
```

### Example Response
```json
{
  "answer": "Echo: What is your name?"
}
```

### Error Codes
- `429 Too Many Requests`: Rate limit exceeded.
- `401 Unauthorized`: Invalid or missing API key (if auth required).

---

## For Bot Developers
- Use the `/api/a2a/ask` endpoint for direct bot-to-bot communication.
- Respect rate limits and authentication requirements.
- See `appsettings.json` for configuration options.
