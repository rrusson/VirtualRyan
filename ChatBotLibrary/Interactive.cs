using Azure;
using Azure.AI.Inference;

namespace ChatBotLibrary
{
	public class Interactive
	{
		public async Task Chat()
		{
			string key = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN not found!");
			var endpoint = new Uri("https://models.github.ai/inference");
			var credential = new AzureKeyCredential(key);
			//var model = "microsoft/Phi-4";
			string model = "openai/gpt-4.1";

			var client = new ChatCompletionsClient(endpoint, credential, new ChatCompletionsClientOptions());

			// Add a system role message before the user message
			var systemMsg = new ChatRequestSystemMessage("You are a doctor with a weird sense of humor and a love of puns.");
			var msg = new ChatRequestUserMessage("Give me 5 good reasons why I should exercise every day.");

			// Add messages to the request options
			var requestOptions = new ChatCompletionsOptions()
			{
				Messages = { systemMsg, msg },
				Temperature = 1.0f,
				NucleusSamplingFactor = 1.0f,
				MaxTokens = 1000,
				Model = model
			};

			StreamingResponse<StreamingChatCompletionsUpdate> response = await client.CompleteStreamingAsync(requestOptions).ConfigureAwait(false);

			await foreach (StreamingChatCompletionsUpdate chatUpdate in response)
			{
				if (!string.IsNullOrEmpty(chatUpdate.ContentUpdate))
				{
					Console.WriteLine(chatUpdate.ContentUpdate);
				}
			}

			Console.WriteLine("");
		}
	}
}
