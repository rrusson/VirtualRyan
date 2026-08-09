namespace VirtualRyan.Server.Controllers
{
	public partial class ChatController
	{
		public class ChatResponse
		{
			public string Answer { get; set; } = string.Empty;

			public string ConversationId { get; set; } = string.Empty;
		}
	}
}
