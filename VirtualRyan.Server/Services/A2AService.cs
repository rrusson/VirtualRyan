using A2A;

using ChatBotLibrary;

namespace VirtualRyan.Server.Services
{
	/// <summary>
	/// Service for handling A2A (Application-to-Application) communication with VirtualRyan
	/// </summary>
	public class A2AService : IAgentHandler
	{
		private const string _jsonContentType = "application/json";
		private const string _textPlain = "text/plain";

		private readonly ILogger<A2AService> _logger;
		private readonly IConfiguration _configuration;

		public A2AService(ILogger<A2AService> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
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

			string baseSystemPrompt = _configuration["SystemPrompt"] ?? string.Empty;
			string a2aSystemPrompt = $"""
				{baseSystemPrompt}
				NOTE: This request is from another AI agent in an A2A (Agent-to-Agent) context. Provide a clear, direct response suitable for agent-to-agent communication.
				""";

			try
			{
				var llmConfig = new LlmConfig
				{
					LlmEndpoint = _configuration["Llm:Endpoint"],
					LlmModel = _configuration["Llm:Model"],
					LlmApiKey = _configuration["Llm:ApiKey"]
				};
				var chatClient = new RyanChat(a2aSystemPrompt, llmConfig);
				string response = await chatClient.AskQuestionAsync(question, cancellationToken).ConfigureAwait(false);
				_logger.LogInformation("A2A: INTERACTION\r\nQ: {Question} \r\nA: {Response}", question, response);

				await responder.ReplyAsync(response, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("A2A: Request cancelled for context {ContextId}", context.ContextId);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "A2A: Error processing task for context {ContextId}", context.ContextId);
				var updater = new TaskUpdater(eventQueue, context.TaskId, context.ContextId);
				// The SDK requires an initial task event before a failure status can be projected.
				await updater.SubmitAsync(cancellationToken).ConfigureAwait(false);
				await updater.FailAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			}
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
				Capabilities = new AgentCapabilities { Streaming = true, PushNotifications = false }
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
				Capabilities = new AgentCapabilities { Streaming = true, PushNotifications = false }
			};
		}
	}
}
