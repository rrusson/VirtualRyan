# Customization Guide
Here's how to personalize this application...

### Configuration Changes
To customize the person's information displayed in the chat interface:

1. **Edit `virtualryan.client/src/personConfig.ts`**
   - Update the following properties in the `defaultPersonConfig` object:
     ```typescript
     const defaultPersonConfig: PersonConfig = {
         firstName: "Your First Name",         // Used in input placeholder
         lastName: "Your Last Name",           // For future use, not displayed
         fullName: "Your Full Name",           // Used in welcome message
         subtitle: "A subtitle for the bot",   // Displayed in chat header
         avatarUrl: "https://your-avatar.com", // URL for the bot's avatar image
     };
     ```
2. **Edit `appsettings.json` in the `VirtualRyan.Server` project**
  - Update the _SystemPrompt_ setting with the system message that controls the bot's behavior:
     ```json
     "SystemPrompt": "Only answer questions about Jane Doe's resume and the provided context with her background, skills, and experience."
     ```
3. **Put your own (plain text) context files (e.g. resume, bio information, etc.) in the `ChatBotLibrary` _Context_ folder.**
 
### Customizable Elements
The following elements will automatically update when you change the configuration:
- **Chat Subtitle**: Shows below the "RESUME BOT" chat header
- **Avatar Image**: The bot's avatar in the chat window
- **Welcome Message**: Mentions the person's full name
- **Input Placeholder**: References the person's first name

### Additional Customization
- **Page Title**: Edit `virtualryan.client/index.html` to change the browser tab title
- **Favicon**: Replace `/Logo.svg` with your own icon file
- **Styling**: Modify CSS files in `virtualryan.client/css/` for visual customization

## Example Configuration
```typescript
const defaultPersonConfig: PersonConfig = {
    firstName: "Jane",
    lastName: "Smith", 
    fullName: "Jane Smith",
    subtitle: "Virtual Jane Smith",
    avatarUrl: "https://example.com/jane-avatar.jpg"
};
```
