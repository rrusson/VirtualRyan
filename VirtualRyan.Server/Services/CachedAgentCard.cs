using A2A;

namespace VirtualRyan.Server.Services
{
	public partial class A2AService
	{
		/// <summary>
		/// Cache entry for an AgentCard with expiration time
		/// </summary>
		private class CachedAgentCard
		{
			public AgentCard AgentCard { get; }

			public DateTime ExpirationTime { get; }

			public CachedAgentCard(AgentCard agentCard, DateTime expirationTime)
			{
				AgentCard = agentCard;
				ExpirationTime = expirationTime;
			}

			public bool IsExpired() => DateTime.UtcNow > ExpirationTime;
		}
	}
}