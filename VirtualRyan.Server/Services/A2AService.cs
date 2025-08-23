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

		public A2AService(ILogger<A2AService> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}

		public void Attach(ITaskManager taskManager)
		{
			taskManager.OnMessageReceived = ProcessMessageAsync;
			taskManager.OnAgentCardQuery = GetAgentCardAsync;
		}

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

			string caller = "Unknown Agent";

			// Get the system prompt and enhance it for A2A context
			string baseSystemPrompt = _configuration["SystemPrompt"] ?? string.Empty;
			string a2aSystemPrompt = $"""
				{baseSystemPrompt}
				NOTE: This request is from another AI agent ({caller}) in an A2A (Application-to-Application) context. Provide a clear, direct response suitable for agent-to-agent communication.
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

		private Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
		{
			_logger.LogInformation("A2A: Providing agent card for URL: {AgentUrl}", agentUrl);

			var agentSection = _configuration.GetSection("A2A:Agent");
			string agentName = agentSection["Name"] ?? "Resume Agent";
			string agentVersion = agentSection["Version"] ?? "1.0.0";
			string agentDescription = agentSection["Description"] ?? "Bot-to-bot communication for accessing professional information.";

			return Task.FromResult(new AgentCard
			{
				Name = agentName,
				Description = agentDescription,
				Url = agentUrl,
				Version = agentVersion,
				DefaultInputModes = ["text"],
				DefaultOutputModes = ["text"],
				Capabilities = new AgentCapabilities { Streaming = true }
			});
		}
	}
}