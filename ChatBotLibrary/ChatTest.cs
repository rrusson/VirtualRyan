using Azure;
using Azure.AI.Inference;

namespace ChatBotLibrary
{
	public class ChatTest
	{
		internal void DoStuff()
		{
			string key = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN not found!");
			var endpoint = new Uri("https://models.github.ai/inference");
			var credential = new AzureKeyCredential(key);
			var model = "microsoft/Phi-4";

			var client = new ChatCompletionsClient(endpoint, credential, new ChatCompletionsClientOptions());

			var requestOptions = new ChatCompletionsOptions()
			{
				Messages =
				{
					new ChatRequestUserMessage("What is the capital of Norway?"),
				},
				Temperature = 1.0f,
				NucleusSamplingFactor = 1.0f,
				MaxTokens = 1000,
				Model = model
			};

			Response<ChatCompletions> response = client.Complete(requestOptions);
			Console.WriteLine(response.Value.Choices.Select(x => x.Message).FirstOrDefault());
		}
	}
}
