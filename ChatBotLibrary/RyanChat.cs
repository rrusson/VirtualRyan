using System.Text;

using Azure;
using Azure.AI.Inference;

namespace ChatBotLibrary
{
	public class RyanChat
	{
        private const string _lmmEndpoint = "https://models.github.ai/inference";
        private readonly string _systemPrompt;

		public RyanChat(string systemPrompt)
		{
			_systemPrompt = systemPrompt;
		}

		public async Task<string> AskQuestionAsync(string[] messages, CancellationToken cancellationToken = default)
		{
			string key = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN not found!");
			var endpoint = new Uri(_lmmEndpoint);
			var credential = new AzureKeyCredential(key);
			string model = "openai/gpt-4.1";

            var client = new ChatCompletionsClient(endpoint, credential, new ChatCompletionsClientOptions());
			ChatCompletionsOptions requestOptions = GetResumeChatOptions(messages, model);

			var responseText = new StringBuilder(6000);
			
			StreamingResponse<StreamingChatCompletionsUpdate> response = await client.CompleteStreamingAsync(requestOptions, cancellationToken).ConfigureAwait(false);

			await foreach (StreamingChatCompletionsUpdate chatUpdate in response.EnumerateValues().WithCancellation(cancellationToken).ConfigureAwait(false))
			{
				if (!string.IsNullOrEmpty(chatUpdate.ContentUpdate))
				{
					responseText.Append(chatUpdate.ContentUpdate);
					Console.Write(chatUpdate.ContentUpdate);
				}
			}

			return responseText.ToString();
		}

		private ChatCompletionsOptions GetResumeChatOptions(string[] messages, string model)
		{
			// Use the system prompt passed from config in VirtualRyan.Server.
			var systemMsg = new ChatRequestSystemMessage(_systemPrompt);

			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string contextFolderPath = Path.Combine(baseDirectory, "Context");
			string[] contextFiles = TextFileReader.ReadAllTextFiles(contextFolderPath);
			var contextMessages = contextFiles.Select(fileContent => new ChatRequestSystemMessage(fileContent));

			// ??? Should we include previous questions and answers ??? Or just the current question?
			var userMsg = new ChatRequestUserMessage(string.Join(" ", messages));

			List<ChatRequestMessage> combinedMessages = [systemMsg, .. contextMessages, userMsg];

			var requestOptions = new ChatCompletionsOptions()
			{
				Temperature = 1.0f,
				NucleusSamplingFactor = 1.0f,
				MaxTokens = 500,
				Model = model
			};

			combinedMessages.ForEach(msg => requestOptions.Messages.Add(msg));

			return requestOptions;
		}
	}
}
