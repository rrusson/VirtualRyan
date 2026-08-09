using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatBotLibrary
{
	public class RyanChat
	{
		private static readonly HttpClient _httpClient = new();

		private static readonly ConcurrentDictionary<string, Lazy<string>> _contextCache = new();

		private static readonly ConcurrentDictionary<string, FileSystemWatcher> _contextWatchers = new();

		private readonly string _systemPrompt;

		private readonly LlmConfig _llmConfig;

		public RyanChat(string systemPrompt, LlmConfig? llmConfig = null)
		{
			if (string.IsNullOrWhiteSpace(systemPrompt))
			{
				throw new ArgumentException("System prompt is required.", nameof(systemPrompt));
			}

			_systemPrompt = systemPrompt;
			_llmConfig = new LlmConfig
			{
				LlmEndpoint = string.IsNullOrWhiteSpace(llmConfig?.LlmEndpoint) ? "http://localhost:11434/v1/chat/completions" : llmConfig.LlmEndpoint,
				LlmModel = string.IsNullOrWhiteSpace(llmConfig?.LlmModel) ? "llama3.2:1b" : llmConfig.LlmModel,
				LlmApiKey = string.IsNullOrWhiteSpace(llmConfig?.LlmApiKey) ? null : llmConfig.LlmApiKey
			};
		}

		/// <summary>
		/// Sends a single question to the LLM without any conversation history.
		/// </summary>
		public Task<string> AskQuestionAsync(string question, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(question))
			{
				throw new ArgumentException("Question is required.", nameof(question));
			}

			return SendQuestionAsync(question, [], cancellationToken);
		}

		/// <summary>
		/// Sends a question along with prior conversation history and returns the updated history (trimmed to 10 exchanges).
		/// </summary>
		public async Task<(string Answer, IReadOnlyList<ChatMessage> UpdatedHistory)> AskQuestionWithHistoryAsync(
			string question,
			IReadOnlyList<ChatMessage>? history,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(question))
			{
				throw new ArgumentException("Question is required.", nameof(question));
			}

			IReadOnlyList<ChatMessage> priorHistory = history ?? [];
			string answer = await SendQuestionAsync(question, priorHistory, cancellationToken).ConfigureAwait(false);

			List<ChatMessage> updatedHistory =
			[
				.. priorHistory,
				new ChatMessage("user", question),
				new ChatMessage("assistant", answer)
			];

			// Keep only the most recent 10 exchanges (user + assistant = 20 messages) so prompts stay small and latency stays low.
			if (updatedHistory.Count > 20)
			{
				updatedHistory.RemoveRange(0, updatedHistory.Count - 20);
			}

			return (answer, updatedHistory);
		}

		private async Task<string> SendQuestionAsync(string question, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, _llmConfig?.LlmEndpoint)
			{
				Content = JsonContent.Create(GetRequestPayload(question, history))
			};

			if (!string.IsNullOrWhiteSpace(_llmConfig?.LlmApiKey))
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _llmConfig.LlmApiKey);
			}

			using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
			string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

			return !response.IsSuccessStatusCode
				? throw new InvalidOperationException($"LLM request failed with status code {(int)response.StatusCode}: {responseBody}")
				: ExtractResponseContent(responseBody);
		}

		private object GetRequestPayload(string question, IReadOnlyList<ChatMessage> history)
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string contextFolderPath = Path.Combine(baseDirectory, "Context");
			string contextText = GetCachedContextText(contextFolderPath);

			List<object> requestMessages =
			[
				new { role = "system", content = _systemPrompt },
				.. history.Select(entry => new { role = entry.Role, content = entry.Content }),
				new { role = "user", content = question }
			];

			// Context stays at the front so the prompt prefix is stable across turns, which lets providers reuse cached prompt processing.
			if (!string.IsNullOrWhiteSpace(contextText))
			{
				requestMessages.Insert(1, new { role = "system", content = contextText });
			}

			return new
			{
				model = _llmConfig?.LlmModel,
				temperature = 0.7,
				top_p = 1.0,
				max_tokens = 300,
				messages = requestMessages
			};
		}

		// Context files change rarely, so their contents are read from disk once and cached; a watcher invalidates the cache when they change.
		private static string GetCachedContextText(string contextFolderPath)
		{
			_contextWatchers.GetOrAdd(contextFolderPath, CreateContextWatcher);

			try
			{
				return _contextCache.GetOrAdd(contextFolderPath, path => new Lazy<string>(() => LoadContextText(path))).Value;
			}
			catch
			{
				// Evict so a transient failure (e.g. missing folder) is retried on the next request instead of being cached by the Lazy forever.
				_contextCache.TryRemove(contextFolderPath, out _);
				throw;
			}
		}

		private static string LoadContextText(string contextFolderPath)
		{
			return string.Join("\n-----\n", TextFileReader.ReadAllTextFiles(contextFolderPath));
		}

		private static FileSystemWatcher CreateContextWatcher(string contextFolderPath)
		{
			var watcher = new FileSystemWatcher(contextFolderPath, "*.txt")
			{
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
				EnableRaisingEvents = true
			};

			watcher.Changed += (_, _) => _contextCache.TryRemove(contextFolderPath, out _);
			watcher.Created += (_, _) => _contextCache.TryRemove(contextFolderPath, out _);
			watcher.Deleted += (_, _) => _contextCache.TryRemove(contextFolderPath, out _);
			watcher.Renamed += (_, _) => _contextCache.TryRemove(contextFolderPath, out _);

			return watcher;
		}

		private static string ExtractResponseContent(string responseBody)
		{
			using JsonDocument json = JsonDocument.Parse(responseBody);

			if (!json.RootElement.TryGetProperty("choices", out JsonElement choices) || choices.GetArrayLength() == 0)
			{
				throw new InvalidOperationException("LLM response did not contain any choices.");
			}

			JsonElement firstChoice = choices[0];

			if (firstChoice.TryGetProperty("message", out JsonElement messageElement)
				&& messageElement.TryGetProperty("content", out JsonElement contentElement)
				&& !string.IsNullOrWhiteSpace(contentElement.GetString()))
			{
				return contentElement.GetString()!;
			}
			else
			{
				return firstChoice.TryGetProperty("text", out JsonElement textElement)
					&& !string.IsNullOrWhiteSpace(textElement.GetString())
				? textElement.GetString()!
				: throw new InvalidOperationException("LLM response was empty.");
			}
		}
	}
}
