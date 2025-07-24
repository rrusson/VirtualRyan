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
   git clone
2. Install dependencies:
   ```bash
   npm install
3. Set a system environment variable for your `GITHUB_TOKEN`
4. Replace files in the `ChatbotLibrary\Context` folder with your own resume, bio, etc.
5. Replace instances of "Ryan" in the code with your name (especially the system prompt in `RyanChat.cs`). TODO: create list and parameterize as much as possible.