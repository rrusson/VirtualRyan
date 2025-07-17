import { useState, useRef } from 'react';
import './resume-chat.css';

function ResumeChat() {
    const [chatQuestion, setChatQuestion] = useState<string>('');
    const [chatResponse, setChatResponse] = useState<string>('');
    const [loading, setLoading] = useState<boolean>(false);
    const formRef = useRef<HTMLFormElement>(null);

    const submitQuestion = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        console.log('submitQuestion called with:', chatQuestion);
        if (!chatQuestion.trim()) {
            console.log('No question entered.');
            return;
        }

        setLoading(true);
        setChatResponse('');

        try {
            const response = await fetch('/Chat/AskQuestion', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ question: chatQuestion }),
            });

            if (response.ok) {
                const result = await response.text();
                console.log('HTTP response received:', result);
                setChatResponse(result);
            } else {
                const errorText = await response.text();
                console.error('HTTP error:', errorText);
                setChatResponse('Error getting response: ' + errorText);
            }
        } catch (error) {
            console.error('Fetch error:', error);
            setChatResponse('Error getting response: ' + error);
        } finally {
            setLoading(false);
        }
    };

    // Listen for Enter (Return) key and submit if not Shift+Enter
    const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            if (formRef.current && chatQuestion.trim() && !loading) {
                formRef.current.requestSubmit();
            }
        }
    };

    return (
        <div class="chat">
            <form ref={formRef}
                onSubmit={submitQuestion}
                style={{ marginBottom: '2rem' }}>
		        <div class="chat-title">
			        <h1>Ryan Russon</h1>
			        <h2>Full-Stack Slacker</h2>
			        <figure className="avatar">
				        <img src="https://avatars.githubusercontent.com/u/653188?v=4" />
			        </figure>
		        </div>
		        <div class="messages">
			        <div class="messages-content"></div>
		        </div>
		        <div class="message-box">
			        <textarea
                        type="text" 
                        class="message-input"
                        id="chatQuestion"
                        value={chatQuestion}
                        onChange={(e) => setChatQuestion(e.target.value)}
                        onKeyDown={handleKeyDown}
                        name="chatQuestion"
                        rows={8}
                        cols={80}
                        placeholder="Ask a question about Ryan's resume..." />
                    <button type="submit" class="message-submit" disabled={loading || !chatQuestion.trim()}>Send</button>
		        </div>
                {loading && (
                    <div className="loading-container">
                        <div className="spinner"></div>
                        <span>Loading response...</span>
                    </div>
                )}

                {chatResponse && !loading && (
                    <div className="response">
                        {chatResponse}
                    </div>
                )}
            </form>
        </div>
    );
}
export default ResumeChat;