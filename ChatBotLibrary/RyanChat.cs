using System.Text;

using Azure;
using Azure.AI.Inference;

namespace ChatBotLibrary
{
	public class RyanChat
	{
		public async Task<string> AskQuestionAsync(string[] messages)
		{
			string key = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN not found!");
			var endpoint = new Uri("https://models.github.ai/inference");
			var credential = new AzureKeyCredential(key);
			//var model = "microsoft/Phi-4";
			string model = "openai/gpt-4.1";

			var client = new ChatCompletionsClient(endpoint, credential, new ChatCompletionsClientOptions());
			ChatCompletionsOptions requestOptions = GetResumeChatOptions(messages, model);

			var responseText = new StringBuilder(6000);
			StreamingResponse<StreamingChatCompletionsUpdate> response = await client.CompleteStreamingAsync(requestOptions).ConfigureAwait(false);

			await foreach (StreamingChatCompletionsUpdate chatUpdate in response)
			{
				if (!string.IsNullOrEmpty(chatUpdate.ContentUpdate))
				{
					responseText.Append(chatUpdate.ContentUpdate);
					Console.Write(chatUpdate.ContentUpdate);
				}
			}

			return responseText.ToString();
		}

		private static ChatCompletionsOptions GetResumeChatOptions(string[] messages, string model)
		{
			// Add a system role message before the user message
			var systemMsg = new ChatRequestSystemMessage(@"Answer questions about Ryan Russon, a senior full-stack .NET software developer seeking employment.
				Answer questions based on the included resume and background information context.
				Be succinct, without extra phrasing like 'based off his resume...',
				if asked about a missing skill mention he's experimented with most technologies and a quick learner, 
				and emphasize his strong work ethic and reliability when apropos.");

			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string contextFolderPath = Path.Combine(baseDirectory, "Context");
			string[] contextFiles = TextFileReader.ReadAllTextFiles(contextFolderPath);
			var contextMessages = contextFiles.Select(fileContent => new ChatRequestSystemMessage(fileContent));

			// ??? Should we include previous questions and answers ??? Or just the current question (will the bot re-answer?).
			var userMsg = new ChatRequestUserMessage(string.Join(" ", messages));

			List<ChatRequestMessage> combinedMessages = new() { systemMsg };
			combinedMessages.AddRange(contextMessages);
			combinedMessages.Add(userMsg);

			// Add messages to the request options
			var requestOptions = new ChatCompletionsOptions()
			{
				Temperature = 1.0f,
				NucleusSamplingFactor = 1.0f,
				MaxTokens = 1000,
				Model = model
			};

			combinedMessages.ForEach(msg => requestOptions.Messages.Add(msg));

			return requestOptions;
		}
	}
}
