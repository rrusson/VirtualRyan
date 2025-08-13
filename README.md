# BACKGROUND
This is my playground for various LLM and AI experiments. Initially it will be a resume question-answering chatbot, maybe adding the ability to convincingly interview for a job through A2A later.
Eventually I'd like to vectorize every e-mail, chat, essay, social media post, etc. I've archived and use RAG or perhaps model fine-tuning to create a more complete "Virtual Ryan".

The current implementation is using [GitHub Models](https://github.com/marketplace/models) to provide speed and a large context window for all the background information in my resume and documentation.

Thanks to the UI geniuses whose work I pilfered to give my project some polish.
* The Max Headroom-esque background: 
https://codepen.io/0x04/pen/DoEELw by Oliver Kühn
* Chat interface: 
https://codepen.io/supah/pen/jqOBqp by Fabio Ottaviani


# INSTALLATION
1. Clone the repository:
   ```bash
   git clone https://github.com/rrusson/VirtualRyan.git
2. Install dependencies:
   ```bash
   npm install
3. Set up a `GITHUB_TOKEN` for your account, if you don't have one.    
    - Go to https://github.com/marketplace/models/azure-openai/gpt-4-1 and click the green _"Use this model"_ button to get started.
    - The token must include the `models:read` permission for a fine-grained Personal Access Token or use a Classic token with repo scope.
    - More information about tokens [here](https://docs.github.com/en/actions/tutorials/authenticate-with-github_token).
4. Set a system environment variable for your `GITHUB_TOKEN` ([instructions for Windows 10](https://www.wikihow.com/Create-an-Environment-Variable-in-Windows-10))
5. Follow the [customization guide](CUSTOMIZATION.md) to personalize the application