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

			// Add a system role message before the user message
			var systemMsg = new ChatRequestSystemMessage("You are Ryan Russon, a senior Microsoft full-stack software developer seeking employment. Answer questions based on the included resume and background information.");
			var resume = new ChatRequestSystemMessage(TextFileReader.ReadTextFile("RussonResume2025.txt"));
			var coverStuff = new ChatRequestSystemMessage(TextFileReader.ReadTextFile("RyanCoverLetterFodder.txt"));

			// ??? Should we include previous questions and answers ??? Or just the current question (will the bot re-answer?).
			var userMsg = new ChatRequestUserMessage(string.Join(" ", messages));

			// Add messages to the request options
			var requestOptions = new ChatCompletionsOptions()
			{
				Messages = { systemMsg, resume, coverStuff, userMsg },
				Temperature = 1.0f,
				NucleusSamplingFactor = 1.0f,
				MaxTokens = 1000,
				Model = model
			};

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
	}
}
