using ChatBotLibrary;

using Microsoft.Extensions.Caching.Memory;

namespace VirtualRyan.Server.Services
{
	/// <summary>
	/// Stores per-conversation chat history in memory, keyed by conversation ID.
	/// Entries expire after 30 minutes of inactivity and are capped at 10 exchanges (20 messages).
	/// </summary>
	public class ConversationHistoryService
	{
		private static readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

		private readonly IMemoryCache _cache;

		public ConversationHistoryService(IMemoryCache cache)
		{
			_cache = cache;
		}

		/// <summary>
		/// Returns a snapshot of the conversation's history, or an empty list for a new conversation.
		/// </summary>
		public IReadOnlyList<ChatMessage> GetHistory(string conversationId)
		{
			if (string.IsNullOrWhiteSpace(conversationId))
			{
				return [];
			}

			return _cache.TryGetValue(conversationId, out List<ChatMessage>? history) && history is not null
				? [.. history]
				: [];
		}

		/// <summary>
		/// Stores the updated history for a conversation, trimming to the 10 most recent exchanges.
		/// </summary>
		public void SaveHistory(string conversationId, IReadOnlyList<ChatMessage> history)
		{
			if (string.IsNullOrWhiteSpace(conversationId))
			{
				return;
			}

			List<ChatMessage> trimmedHistory = [.. history];

			if (trimmedHistory.Count > 20)
			{
				trimmedHistory.RemoveRange(0, trimmedHistory.Count - 20);
			}

			_cache.Set(conversationId, trimmedHistory, new MemoryCacheEntryOptions
			{
				SlidingExpiration = _expiration
			});
		}
	}
}
