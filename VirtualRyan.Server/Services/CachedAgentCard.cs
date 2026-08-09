using A2A;

namespace VirtualRyan.Server.Services
{
	public partial class A2AService
	{
		/// <summary>
		/// Cache entry for an AgentCard with expiration time
		/// </summary>
		private sealed class CachedAgentCard(AgentCard agentCard, DateTime expirationTime)
		{
			public AgentCard AgentCard { get; } = agentCard;

			public DateTime ExpirationTime { get; } = expirationTime;

			public bool IsExpired() => DateTime.UtcNow > ExpirationTime;
		}
	}
}