using ChatBotLibrary;

using Microsoft.AspNetCore.Mvc;

using VirtualRyan.Server.Services;

namespace VirtualRyan.Server.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public partial class ChatController : ControllerBase
	{
		private readonly ILogger<ChatController> _logger;
		private readonly IConfiguration _configuration;
		private readonly LlmConfig _llmConfig;

		public ChatController(ILogger<ChatController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
			_logger.LogInformation("ChatController initialized");

			_llmConfig = new LlmConfig
			{
				LlmEndpoint = configuration["Llm:Endpoint"],
				LlmModel = configuration["Llm:Model"],
				LlmApiKey = configuration["Llm:ApiKey"]
			};
		}

		[HttpPost("AskQuestion")]
		public async Task<ActionResult<ChatResponse>> AskQuestion([FromBody] ChatRequest request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request?.Question))
			{
				_logger.LogWarning("Received an empty or null question.");
				throw new ArgumentException("Question cannot be null or empty.", nameof(request));
			}

			string conversationId = string.IsNullOrWhiteSpace(request.ConversationId) ? Guid.NewGuid().ToString("N") : request.ConversationId;
			var sanitizedConversationIdForLog = TextSanitizer.Sanitize(conversationId);
			var sanitizedQuestionForLog = TextSanitizer.Sanitize(request.Question);
			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
			_logger.LogInformation("{Ip} RECEIVED QUESTION for conversation {ConversationId}: {Question}", ip, sanitizedConversationIdForLog, sanitizedQuestionForLog);

			try
			{
				string systemPrompt = _configuration["SystemPrompt"] ?? string.Empty;
				var chatClient = new RyanChat(systemPrompt, _llmConfig);
				string response = await chatClient.AskQuestionAsync([request.Question], cancellationToken).ConfigureAwait(false);

				_logger.LogInformation("RETURNING RESPONSE: {Response}", response);

				return Ok(new ChatResponse
				{
					Answer = response,
					ConversationId = conversationId
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERROR in AskQuestion for question: {Question}", sanitizedQuestionForLog);

				return Ok(new ChatResponse
				{
					Answer = "Sorry, an error occurred while processing your question.",
					ConversationId = conversationId
				});
			}
		}
	}
}
