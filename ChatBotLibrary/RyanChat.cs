using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatBotLibrary
{
	public class RyanChat
	{
		private static readonly HttpClient _httpClient = new();

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

		public async Task<string> AskQuestionAsync(string[] messages, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(messages);

			if (messages.Length == 0)
			{
				throw new ArgumentException("At least one message is required.", nameof(messages));
			}

			using var request = new HttpRequestMessage(HttpMethod.Post, _llmConfig?.LlmEndpoint)
			{
				Content = JsonContent.Create(GetRequestPayload(messages))
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

		private object GetRequestPayload(string[] messages)
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string contextFolderPath = Path.Combine(baseDirectory, "Context");
			string[] contextFiles = TextFileReader.ReadAllTextFiles(contextFolderPath);
			string userMessage = string.Join(" ", messages.Where(message => !string.IsNullOrWhiteSpace(message)));

			if (string.IsNullOrWhiteSpace(userMessage))
			{
				throw new ArgumentException("User message content is required.", nameof(messages));
			}

			List<object> requestMessages =
			[
				new { role = "system", content = _systemPrompt },
				.. contextFiles.Select(fileContent => new { role = "system", content = fileContent }),
				new { role = "user", content = userMessage }
			];

			return new
			{
				model = _llmConfig?.LlmModel,
				temperature = 1.0,
				top_p = 1.0,
				max_tokens = 500,
				messages = requestMessages
			};
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
