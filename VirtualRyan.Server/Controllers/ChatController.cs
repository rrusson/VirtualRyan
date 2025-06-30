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
		}

		public class ChatRequest
		{
			public string? Question { get; set; }
		}

		[HttpPost("AskQuestion")]
		public async Task<string> AskQuestion([FromBody] ChatRequest request)
		{
			_logger.LogInformation("Received question: {Question}", request.Question);

			RyanChat chatClient = new RyanChat();
			string response = await chatClient.AskQuestionAsync(new string[] { request.Question }).ConfigureAwait(false);

			_logger.LogInformation($"Returning response: {response}");
			return response;
		}
	}
}
