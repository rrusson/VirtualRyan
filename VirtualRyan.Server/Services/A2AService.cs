using System.Collections.Concurrent;
using System.Net;

using A2A;

using ChatBotLibrary;

namespace VirtualRyan.Server.Services
{
	/// <summary>
	/// Service for handling A2A (Application-to-Application) communication with VirtualRyan
	/// </summary>
	public partial class A2AService : IAgentHandler
	{
		private const string _unknown = "unknown";
		private const string _jsonContentType = "application/json";
        private const string _textPlain = "text/plain";

        private readonly ILogger<A2AService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly RateLimitingService _rateLimitingService;

		// Cache for storing AgentCard information by hostname
		private readonly ConcurrentDictionary<string, CachedAgentCard> _agentCardCache = new();
		private readonly TimeSpan _cacheExpirationTime = TimeSpan.FromMinutes(5);

		public A2AService(
			ILogger<A2AService> logger,
			IConfiguration configuration,
			IHttpContextAccessor httpContextAccessor,
			RateLimitingService rateLimitingService)
		{
			_logger = logger;
			_configuration = configuration;
			_httpContextAccessor = httpContextAccessor;
			_rateLimitingService = rateLimitingService;
		}

		/// <summary>
		/// Handles an incoming A2A message request
		/// </summary>
		/// <param name="context">The request context containing the incoming message</param>
		/// <param name="eventQueue">The event queue used to send responses back to the caller</param>
		/// <param name="cancellationToken"></param>
		public async Task ExecuteAsync(RequestContext context, AgentEventQueue eventQueue, CancellationToken cancellationToken)
		{
			string question = context.UserText ?? string.Empty;
			var responder = new MessageResponder(eventQueue, context.ContextId);

			if (string.IsNullOrWhiteSpace(question))
			{
				await responder.ReplyAsync("Error: Empty request", cancellationToken).ConfigureAwait(false);
				return;
			}

			var callingAgent = await GetCallerAgentCardAsync(cancellationToken).ConfigureAwait(false);
			string caller = callingAgent?.Name ?? _unknown;

			// Get the system prompt and enhance it for A2A context
			string baseSystemPrompt = _configuration["SystemPrompt"] ?? string.Empty;
			string a2aSystemPrompt = $"""
				{baseSystemPrompt}
				NOTE: This request is from another AI agent ({caller}) in an A2A (Agent-to-Agent) context. Provide a clear, direct response suitable for agent-to-agent communication.
				""";

			try
			{
				var chatClient = new RyanChat(a2aSystemPrompt);
				string response = await chatClient.AskQuestionAsync([question], cancellationToken).ConfigureAwait(false);
				_logger.LogInformation("A2A: INTERACTION\r\nQ: {Question} \r\nA: {Response}", question, response);

				await responder.ReplyAsync(response, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("A2A: Request cancelled for context {ContextId}", context.ContextId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "A2A: Error processing task from {Caller}: {Question}", caller, question);
				await responder.ReplyAsync("Error processing request", cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Handles cancellation of an in-flight A2A request
		/// </summary>
		/// <param name="context">The request context for the task being cancelled</param>
		/// <param name="eventQueue">The event queue</param>
		/// <param name="cancellationToken"></param>
		public Task CancelAsync(RequestContext context, AgentEventQueue eventQueue, CancellationToken cancellationToken)
		{
			_logger.LogInformation("A2A: Task cancelled for context {ContextId}", context.ContextId);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Builds and returns this agent's AgentCard from the given configuration
		/// </summary>
		/// <param name="configuration">Application configuration</param>
		/// <returns>An AgentCard describing this agent's capabilities</returns>
		public static AgentCard BuildAgentCard(IConfiguration configuration)
		{
			var agentConfig = configuration.GetSection("A2A:Agent");
			string agentUrl = agentConfig["Url"] ?? "https://ai.rrusson.com";
			string agentName = agentConfig["Name"] ?? "Resume Question Agent";
			string agentDescription = agentConfig["Description"] ?? "Bot-to-bot communication for accessing professional CV information.";
			var provider = new AgentProvider
			{
				Organization = agentConfig["Provider"] ?? agentName,
				Url = agentConfig["ProviderUrl"] ?? agentUrl
			};

			List<AgentSkill> skills =
			[
				new AgentSkill
				{
					Id = "resume-query",
					Name = "Ask a question about my resume and qualifications",
					Description = agentDescription,
					Tags = ["resume", "recruitment", "jobs", "hiring", "vocational", "skills", "information"],
					InputModes = [_textPlain, _jsonContentType],
					OutputModes = [_textPlain, _jsonContentType],
					Examples = ["How many years experience programming in C#?"]
				}
			];

			return new AgentCard
			{
				Name = agentName,
				Description = agentDescription,
				Provider = provider,
				DocumentationUrl = agentConfig["DocumentationUrl"] ?? agentUrl,
				Skills = skills,
				IconUrl = agentConfig["IconUrl"] ?? "https://ai.rrusson.com/favicon.ico",
				Version = agentConfig["Version"] ?? "1.0.0",
				DefaultInputModes = [_textPlain, _jsonContentType],
				DefaultOutputModes = [_textPlain, _jsonContentType],
				SupportedInterfaces =
				[
					new AgentInterface
					{
						Url = $"{agentUrl}/a2a",
						ProtocolBinding = ProtocolBindingNames.JsonRpc
					},
					new AgentInterface
					{
						Url = $"{agentUrl}/a2a",
						ProtocolBinding = ProtocolBindingNames.HttpJson
					}
				],
				Capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false }
			};
		}

		/// <summary>
		/// Builds and returns this agent's AgentCard for A2A communication
		/// </summary>
		/// <param name="agentUrl">The base URL of this agent</param>
		/// <returns>An AgentCard describing this agent's capabilities</returns>
		public AgentCard BuildAgentCard(string agentUrl)
		{
			_logger.LogInformation("A2A: Providing agent card for URL: {AgentUrl}", agentUrl);

			var agentConfig = _configuration.GetSection("A2A:Agent");
			string agentName = agentConfig["Name"] ?? "Resume Question Agent";
			string agentDescription = agentConfig["Description"] ?? "Bot-to-bot communication for accessing professional CV information.";
			var provider = new AgentProvider
			{
				Organization = agentConfig["Provider"] ?? agentName,
				Url = agentConfig["ProviderUrl"] ?? agentUrl
			};

			List<AgentSkill> skills =
			[
				new AgentSkill
				{
					Id = "resume-query",
					Name = "Ask a question about my resume and qualifications",
					Description = agentDescription,
					Tags = ["resume", "recruitment", "jobs", "hiring", "vocational", "skills", "information"],
					InputModes = [_textPlain, _jsonContentType],
					OutputModes = [_textPlain, _jsonContentType],
					Examples = ["How many years experience programming in C#?"]
				}
			];

			return new AgentCard
			{
				Name = agentName,
				Description = agentDescription,
				Provider = provider,
				DocumentationUrl = agentConfig["DocumentationUrl"] ?? agentUrl,
				Skills = skills,
				IconUrl = agentConfig["IconUrl"] ?? "https://ai.rrusson.com/favicon.ico",
				Version = agentConfig["Version"] ?? "1.0.0",
				DefaultInputModes = [_textPlain, _jsonContentType],
				DefaultOutputModes = [_textPlain, _jsonContentType],
				SupportedInterfaces =
				[
					new AgentInterface
					{
						Url = $"{agentUrl}/a2a",
						ProtocolBinding = ProtocolBindingNames.JsonRpc
					},
					new AgentInterface
					{
						Url = $"{agentUrl}/a2a",
						ProtocolBinding = ProtocolBindingNames.HttpJson
					}
				],
				Capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false }
			};
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
				string remoteIp = httpContext?.Connection.RemoteIpAddress?.ToString() ?? _unknown;
				string scheme = httpContext?.Request?.Scheme ?? "https";
				string host = httpContext?.Request?.Host.Value ?? _unknown;

				if (host == _unknown || IsLocalRequest(httpContext) || host.StartsWith("localhost"))
				{
					//No local requests; infinite loops are not fun
					return null;
				}

				AgentCard requesterCard = await GetAgentCard($"{scheme}://{host}", cancellationToken).ConfigureAwait(false);

				var rateLimitInformation = _rateLimitingService.GetRateLimitInfo(remoteIp);
				LogAgentRequest(requesterCard, remoteIp, host, rateLimitInformation);

				return requesterCard;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Trying to get A2A caller info went sideways.");
				return null;
			}
		}

		/// <summary>
		/// Gets the AgentCard for a given host, using a cached version if available
		/// </summary>
		/// <param name="hostUri">host to attempt to fetch AgentCard from</param>
		/// <param name="cancellationToken"></param>
		/// <returns>AgentCard for the host</returns>
		private async Task<AgentCard> GetAgentCard(string hostUri, CancellationToken cancellationToken)
		{
			string sanitizedHostUri = TextSanitizer.Sanitize(hostUri);
			if (_agentCardCache.TryGetValue(hostUri, out var cachedCard) && !cachedCard.IsExpired())
			{
				_logger.LogDebug("A2A: Using cached agent card for host: {Host}", sanitizedHostUri);
				return cachedCard.AgentCard;
			}

			// Not in cache or expired, fetch
			_logger.LogDebug("A2A: Fetching agent card for host: {Host}", sanitizedHostUri);
			var resolver = new A2ACardResolver(new Uri(hostUri));
			AgentCard requesterCard = await resolver.GetAgentCardAsync(cancellationToken).ConfigureAwait(false);
			requesterCard.Name ??= _unknown;
			requesterCard.Description ??= "No description provided";

			_agentCardCache[hostUri] = new CachedAgentCard(requesterCard, DateTime.UtcNow.Add(_cacheExpirationTime));
			return requesterCard;
		}

		/// <summary>
		/// Logs information about an agent request including rate limit details
		/// </summary>
		private void LogAgentRequest(AgentCard agentCard, string remoteIp, string host, RateLimitInfo rateLimitInfo)
		{
			_logger.LogInformation(
				"A2A: Request from {Caller} ({Desc}) at {RemoteIp} ({Host})\r\nA2A: Rate limits: {PerMinuteRemaining}/{PerMinuteLimit} per minute, {PerDayRemaining}/{PerDayLimit} per day",
				agentCard.Name,
				agentCard.Description,
				remoteIp,
				host,
				rateLimitInfo.PerMinuteRemaining,
				rateLimitInfo.PerMinuteLimit,
				rateLimitInfo.PerDayRemaining,
				rateLimitInfo.PerDayLimit
			);
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
