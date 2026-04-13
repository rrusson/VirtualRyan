using ChatBotLibrary;

using Microsoft.AspNetCore.Mvc;

namespace VirtualRyan.Server.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ChatController : ControllerBase
	{
		private readonly ILogger<ChatController> _logger;
		private readonly IConfiguration _configuration;

		public ChatController(ILogger<ChatController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
			_logger.LogInformation("ChatController initialized");
		}

		public class ChatRequest
		{
			public string? Question { get; set; }
		}

		[HttpPost("AskQuestion")]
		public async Task<string> AskQuestion([FromBody] ChatRequest request)
		{
			if (string.IsNullOrWhiteSpace(request?.Question))
			{
				_logger.LogWarning("Received an empty or null question.");
				throw new ArgumentException("Question cannot be null or empty.", nameof(request));
			}

			var sanitizedQuestionForLog = request.Question.Replace("\r", "").Replace("\n", "");
			var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
			_logger.LogInformation("{Ip} RECEIVED QUESTION: {Question}", ip, sanitizedQuestionForLog);

			try
			{
				string systemPrompt = _configuration["SystemPrompt"] ?? string.Empty;
				RyanChat chatClient = new RyanChat(systemPrompt);
				string response = await chatClient.AskQuestionAsync([request.Question]).ConfigureAwait(false);

				_logger.LogInformation("RETURNING RESPONSE: {Response}", response);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERROR in AskQuestion for question: {Question}", sanitizedQuestionForLog);
				return "Sorry, an error occurred while processing your question.";
            }
		}
	}
}
