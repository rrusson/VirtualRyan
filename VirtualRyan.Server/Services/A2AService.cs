using System.Net;

using A2A;

using ChatBotLibrary;

namespace VirtualRyan.Server.Services
{
	/// <summary>
	/// Service for handling A2A (Application-to-Application) communication with VirtualRyan
	/// </summary>
	public class A2AService
	{
		private readonly ILogger<A2AService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public A2AService(
			ILogger<A2AService> logger,
			IConfiguration configuration,
			IHttpContextAccessor httpContextAccessor)
		{
			_logger = logger;
			_configuration = configuration;
			_httpContextAccessor = httpContextAccessor;
		}

		public void Attach(ITaskManager taskManager)
		{
			taskManager.OnMessageReceived = ProcessMessageAsync;
			taskManager.OnAgentCardQuery = GetAgentCardAsync;
		}

		/// <summary>
		/// Returns a response Message to the given A2A MessageSend request
		/// </summary>
		/// <param name="messageSendParams">The request message payload</param>
		/// <param name="cancellationToken"></param>
		/// <returns>A message, with a response as the first Parts element, if successful, otherwise an error</returns>
		private async Task<Message> ProcessMessageAsync(MessageSendParams messageSendParams, CancellationToken cancellationToken)
		{
			var question = messageSendParams.Message.Parts.OfType<TextPart>().FirstOrDefault()?.Text;

			if (string.IsNullOrWhiteSpace(question))
			{
				return new Message
				{
					Role = MessageRole.Agent,
					MessageId = Guid.NewGuid().ToString(),
					ContextId = messageSendParams.Message.ContextId,
					Parts = [new TextPart { Text = "Error: Empty request" }]
				};
			}

			var callingAgent = await GetCallerAgentCardAsync(cancellationToken).ConfigureAwait(false);
			string caller = callingAgent?.Name ?? "unknown";

			// Get the system prompt and enhance it for A2A context
			string baseSystemPrompt = _configuration["SystemPrompt"] ?? string.Empty;
			string a2aSystemPrompt = $"""
				{baseSystemPrompt}
				NOTE: This request is from another AI agent ({caller}) in an A2A (Agent-to-Agent) context. Provide a clear, direct response suitable for agent-to-agent communication.
				""";

			try
			{
				var chatClient = new RyanChat(a2aSystemPrompt);
				string response = await chatClient.AskQuestionAsync([question]).ConfigureAwait(false);
				_logger.LogInformation("A2A: INTERACTION\r\nQ: {Question} \r\nA: {Response}", question, response);

				return new Message
				{
					Role = MessageRole.Agent,
					MessageId = Guid.NewGuid().ToString(),
					ContextId = messageSendParams.Message.ContextId,
					Parts = [new TextPart { Text = response }]
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "A2A: Error processing task from {Caller}: {Question}", caller, question);

				return new Message
				{
					Role = MessageRole.Agent,
					MessageId = Guid.NewGuid().ToString(),
					ContextId = messageSendParams.Message.ContextId,
					Parts = [new TextPart { Text = "Error processing request" }]
				};
			}
		}

		/// <summary>
		/// Returns this agent's AgentCard for A2A communication
		/// </summary>
		/// <param name="agentUrl"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
		{
			_logger.LogInformation("A2A: Providing agent card for URL: {AgentUrl}", agentUrl);

			var agentSection = _configuration.GetSection("A2A:Agent");
			string agentName = agentSection["Name"] ?? "Resume Agent";
			string agentVersion = agentSection["Version"] ?? "1.0.0";
			string agentDescription = agentSection["Description"] ?? "Bot-to-bot communication for accessing professional information.";
			string iconUrl = agentSection["IconUrl"] ?? "https://ai.rrusson.com/favicon.ico";
			var provider = new AgentProvider
			{
				Organization = agentSection["Provider"] ?? agentName,
				Url = agentSection["ProviderUrl"] ?? agentUrl
			};

			var skills = new List<AgentSkill>
			{
				new AgentSkill
				{
					Id = "message/send",
					Name = "Ask a question about my resume and qualifications",
					Description = agentDescription,
					Tags = ["chatbot", "resume", "recruitment", "jobs", "hiring", "professional", "information"],
					InputModes = ["text/plain", "application/json"],
					OutputModes = ["text/plain", "application/json"],
					Examples = ["How many years experience programming in C#?"]
				}
			};

			return Task.FromResult(new AgentCard
			{
				Name = agentName,
				Description = agentDescription,
				Provider = provider,
				Url = agentUrl,
				Skills = skills,
				IconUrl = iconUrl,
				Version = agentVersion,
				DefaultInputModes = ["text/plain", "application/json"],
				DefaultOutputModes = ["text/plain", "application/json"],
				Capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false, StateTransitionHistory = false }
			});
		}


		/// <summary>
		/// Attempts to resolve the caller's agent card from the HTTP context host information
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>The requestor's AgentCard (null if not found or an error occurred)</returns>
		private async Task<AgentCard?> GetCallerAgentCardAsync(CancellationToken cancellationToken)
		{
			try
			{
				var httpContext = _httpContextAccessor.HttpContext;
				string remoteIp = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
				string? host = httpContext?.Request?.Host.Value ?? "unknown";

				if (IsLocalRequest(httpContext) || host.StartsWith("localhost"))
				{
					// Local request, infinite loops are not fun
					return null;
				}

				var resolver = new A2ACardResolver(new Uri($"{host}/.well-known/agent.json"));
				AgentCard requesterCard = await resolver.GetAgentCardAsync(cancellationToken).ConfigureAwait(false);
				requesterCard.Name ??= "unknown";
				requesterCard.Description ??= "No description provided";
				_logger.LogInformation("A2A: Request from {Caller} ({Desc}) at {RemoteIp} ({Host})", requesterCard.Name, requesterCard.Description, remoteIp, host);

				return requesterCard;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Trying to get the A2A caller went sideways.");
				return null;
			}
		}

		private static bool IsLocalRequest(HttpContext? context)
		{
			var remoteIp = context?.Connection.RemoteIpAddress;
			var localIp = context?.Connection.LocalIpAddress;

			if (remoteIp == null)
			{
				return false;
			}

			return IPAddress.IsLoopback(remoteIp) || remoteIp.Equals(localIp);
		}
	}
}