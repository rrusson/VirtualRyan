namespace VirtualRyan.Server.Controllers
{
	public partial class ChatController
	{
		public class ChatRequest
		{
			public string? Question { get; set; }

			public string? ConversationId { get; set; }
		}
	}
}
