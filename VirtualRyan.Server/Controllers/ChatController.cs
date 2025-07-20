using ChatBotLibrary;

using Microsoft.AspNetCore.Mvc;

namespace VirtualRyan.Server.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ChatController : ControllerBase
	{
		private readonly ILogger<ChatController> _logger;

		public ChatController(ILogger<ChatController> logger)
		{
			_logger = logger;
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
				throw new ArgumentException("Question cannot be null or empty.", nameof(request.Question));
			}

			_logger.LogInformation("RECEIVED QUESTION: {Question}", request.Question);

			try
			{
				RyanChat chatClient = new RyanChat();
				string response = await chatClient.AskQuestionAsync([request.Question]).ConfigureAwait(false);
				_logger.LogInformation("RETURNING RESPONSE: {Response}", response);

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERROR in AskQuestion for question: {Question}", request.Question);
				throw;
			}
		}
	}
}
