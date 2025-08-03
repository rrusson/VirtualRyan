import * as React from 'react';
import { useState, useRef, useEffect } from 'react';
import { SpeechToText } from './speechToText';
import { TextToSpeech } from './textToSpeech';
import { Pronunciation } from './pronunciation';

// Types for chat functionality
interface ChatMessage {
	id: string;
	content: string;
	isUser: boolean;
	timestamp: Date;
}

// ChatTitle Component
interface ChatTitleProps {
	isListening: boolean;
	onMicClick: () => void;
}

const ChatTitle: React.FC<ChatTitleProps> = ({ isListening, onMicClick }) => {
	return (
		<div className="chat-title">
			<h1>Resume Bot</h1>
			<h2>Rep. Ryan Russon</h2>
			<figure className="avatar">
				<img src="https://avatars.githubusercontent.com/u/653188?v=4" alt="avatar" />
			</figure>
			<button
				className="mic-button"
				aria-label={isListening ? "Stop voice input" : "Start voice input"}
				type="button"
				onClick={onMicClick}
				style={{
					color: isListening ? '#ff4444' : '',
					animation: isListening ? 'pulse 1s infinite' : 'none'
				}}
			>
				<i className="fa fa-microphone" />
			</button>
		</div>
	);
};

// Message Component - Fixed to match original jQuery structure
interface MessageProps {
	message: ChatMessage;
	showTimestamp: boolean;
}

const Message: React.FC<MessageProps> = ({ message, showTimestamp }) => {
	const formatTime = (date: Date) => {
		return String(date.getHours()).padStart(2, '0') + ':' + String(date.getMinutes()).padStart(2, '0');
	};

	return (
		<div className={`message ${message.isUser ? 'message-personal ' : ''}new`} data-id={message.id}>
			{!message.isUser && (
				<figure className="avatar">
					<img src="https://avatars.githubusercontent.com/u/653188?v=4" alt="avatar" />
				</figure>
			)}
			{message.content}
			{showTimestamp && (
				<div className="timestamp">{formatTime(message.timestamp)}</div>
			)}
		</div>
	);
};

// Loading Message Component
const LoadingMessage: React.FC = () => {
	return (
		<div className="message loading new">
			<figure className="avatar">
				<img src="https://avatars.githubusercontent.com/u/653188?v=4" alt="avatar" />
			</figure>
			<span></span>
		</div>
	);
};

// Messages Display Component
interface MessagesDisplayProps {
	messages: ChatMessage[];
	isLoading: boolean;
}

const MessagesDisplay: React.FC<MessagesDisplayProps> = ({ messages, isLoading }) => {
	const messagesContentRef = useRef<HTMLDivElement>(null);
	const messagesEndRef = useRef<HTMLDivElement>(null);

	const scrollToBottom = () => {
		// Check if we're using mCustomScrollbar
		const windowWithJQuery = window as any;
		if (typeof windowWithJQuery !== 'undefined' && windowWithJQuery.$ && messagesContentRef.current) {
			const $messagesContent = windowWithJQuery.$(messagesContentRef.current);
			if (typeof $messagesContent.mCustomScrollbar === 'function') {
				// Use custom scrollbar API
				try {
					$messagesContent.mCustomScrollbar("update");
					$messagesContent.mCustomScrollbar('scrollTo', 'bottom', {
						scrollInertia: 10,
						timeout: 0
					});
					return;
				} catch (error) {
					console.warn('mCustomScrollbar error:', error);
				}
			}
		}

		// Fallback to native scrolling - scroll the messages-content container
		if (messagesContentRef.current) {
			const container = messagesContentRef.current;
			container.scrollTop = container.scrollHeight;
		}
	};

	// Initialize custom scrollbar when component mounts
	useEffect(() => {
		const initializeScrollbar = () => {
			const windowWithJQuery = window as any;
			if (windowWithJQuery.$ && messagesContentRef.current) {
				const $messagesContent = windowWithJQuery.$(messagesContentRef.current);
				if (typeof $messagesContent.mCustomScrollbar === 'function') {
					$messagesContent.mCustomScrollbar();
					console.log('Custom scrollbar initialized');
				}
			}
		};

		// Try to initialize immediately
		initializeScrollbar();

		// Also try after a short delay in case jQuery/mCustomScrollbar loads later
		const timer = setTimeout(initializeScrollbar, 100);

		return () => clearTimeout(timer);
	}, []);

	useEffect(() => {
		scrollToBottom();
	}, [messages, isLoading]);

	// Group messages to show timestamps only when time changes (like original)
	const getMessagesWithTimestamps = () => {
		return messages.map((message, index) => {
			let showTimestamp = false;

			if (index === 0) {
				showTimestamp = true;
			} else {
				const currentTime = message.timestamp;
				const previousTime = messages[index - 1].timestamp;

				// Show timestamp if minute or hour changed
				if (currentTime.getMinutes() !== previousTime.getMinutes() ||
					currentTime.getHours() !== previousTime.getHours()) {
					showTimestamp = true;
				}
			}

			return { message, showTimestamp };
		});
	};

	return (
		<div className="messages">
			<div className="messages-content" ref={messagesContentRef}>
				<div className="mCSB_container">
					{getMessagesWithTimestamps().map(({ message, showTimestamp }) => (
						<Message key={message.id} message={message} showTimestamp={showTimestamp} />
					))}
					{isLoading && <LoadingMessage />}
					<div ref={messagesEndRef} />
				</div>
			</div>
		</div>
	);
};

// Message Input Component
interface MessageInputProps {
	value: string;
	onChange: (value: string) => void;
	onSubmit: (message: string) => void;
	isLoading: boolean;
}

const MessageInput: React.FC<MessageInputProps> = ({ value, onChange, onSubmit, isLoading }) => {
	const formRef = useRef<HTMLFormElement>(null);
	const inputRef = useRef<HTMLTextAreaElement>(null);

	useEffect(() => {
		if (inputRef.current) {
			inputRef.current.focus();
		}
	}, []);

	const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
		event.preventDefault();
		if (value.trim() && !isLoading) {
			onSubmit(value.trim());
		}
	};

	const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
		if (e.key === 'Enter' && !e.shiftKey) {
			e.preventDefault();
			if (value.trim() && !isLoading) {
				onSubmit(value.trim());
			}
		}
	};

	const handleInputChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
		onChange(e.target.value);
	};

	return (
		<form ref={formRef} onSubmit={handleSubmit} style={{ marginBottom: '0' }}>
			<div className="message-box">
				<textarea
					className="message-input"
					id="chatQuestion"
					ref={inputRef}
					value={value}
					onChange={handleInputChange}
					onKeyDown={handleKeyDown}
					name="chatQuestion"
					rows={8}
					cols={80}
					placeholder="Ask a question about Ryan's resume..."
				/>
				<button type="submit" className="message-submit" disabled={isLoading || !value.trim()}>
					Send
				</button>
			</div>
		</form>
	);
};

// Main ResumeChat Component
function ResumeChat() {
	const [chatQuestion, setChatQuestion] = useState<string>('');
	const [messages, setMessages] = useState<ChatMessage[]>([]);
	const [loading, setLoading] = useState<boolean>(false);
	const [isListening, setIsListening] = useState<boolean>(false);
	const [wasLastInputSpeech, setLastInputWasSpeech] = useState<boolean>(false);

	const speechToTextRef = useRef<SpeechToText | null>(null);
	const textToSpeechRef = useRef<TextToSpeech | null>(null);
	const pronunciationRef = useRef<Pronunciation | null>(null);

	// Initialize TTS and Pronunciation instance, and add welcome message
	useEffect(() => {
		textToSpeechRef.current = new TextToSpeech();
		pronunciationRef.current = new Pronunciation();

		const welcomeMessage: ChatMessage = {
			id: 'welcome-' + Date.now(),
			content: "Hi! I'm a bot that answers questions about Ryan Russon's resume and qualifications. What would you like to know?",
			isUser: false,
			timestamp: new Date()
		};
		setMessages([welcomeMessage]);
	}, []);

	const addChatMessage = (content: string, isUser: boolean): ChatMessage => {
		const message: ChatMessage = {
			id: (isUser ? 'user-' : 'bot-') + Date.now(),
			content,
			isUser,
			timestamp: new Date()
		};
		setMessages(prev => [...prev, message]);
		return message;
	};

	const speakBotResponse = (text: string) => {
		if (textToSpeechRef.current) {
			try {
				// Stop any current speech before starting new one
				textToSpeechRef.current.stop();

				// Use phonetic text for TTS, but original for chat
				const phoneticText = pronunciationRef.current
					? pronunciationRef.current.makeTlasPhonetic(text)
					: text;

				textToSpeechRef.current.speak(phoneticText, {
					rate: 0.9,
					pitch: 1.0,
					volume: 0.8
				});
			} catch (error) {
				console.error('Error converting text to speech:', error);
			}
		}
	};

	const handleMicClick = async () => {
		if (isListening) {
			if (speechToTextRef.current) {
				speechToTextRef.current.stop();
			}
			setIsListening(false);
			return;
		}

		try {
			const stt = new SpeechToText();
			speechToTextRef.current = stt;

			// Set up callbacks
			stt.onStart = () => {
				console.log('Speech recognition started');
				setIsListening(true);
			};

			stt.onEnd = () => {
				console.log('Speech recognition ended');
				setIsListening(false);
			};

			stt.onError = (error: string) => {
				console.error('Speech recognition error:', error);
				setIsListening(false);
				setLastInputWasSpeech(false);

				addChatMessage(`Speech recognition error: ${error}`, false);
			};

			stt.onResult = (transcript: string) => {
				console.log('Speech transcript received:', transcript);

				if (transcript && transcript.trim()) {
					setLastInputWasSpeech(true);
					// Set the transcript in the input field
					setChatQuestion(transcript.trim());

					setTimeout(() => {
						textToSpeechRef.current?.stop();
						handleSubmitQuestion(transcript.trim());
					}, 100);
				}
			};

			stt.listen();

		} catch (error) {
			console.error('Error initializing speech recognition:', error);
			setIsListening(false);
			setLastInputWasSpeech(false);

			addChatMessage('Speech recognition is not supported in this browser or an error occurred.', false);
		}
	};

	const handleSubmitQuestion = async (questionToSubmit?: string) => {
		const question = questionToSubmit || chatQuestion;

		if (!question.trim()) {
			console.log('No question entered.');
			return;
		}

		addChatMessage(question, true);

		setLoading(true);
		const wasInputSpeech = wasLastInputSpeech;
		setChatQuestion('');

		if (!wasInputSpeech) {
			setLastInputWasSpeech(false);
		}

		try {
			const response = await fetch('/Chat/AskQuestion', {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({ question }),
			});

			if (response.ok) {
				const result = await response.text();

				addChatMessage(result, false);

				if (wasInputSpeech) {
					speakBotResponse(result);
					setLastInputWasSpeech(false);
				}
			} else {
				const errorText = await response.text();
				console.error('HTTP error:', errorText);

				addChatMessage('Error getting response: ' + errorText, false);
				setLastInputWasSpeech(false);
			}
		} catch (error) {
			console.error('Fetch error:', error);

			addChatMessage('Error getting response: ' + error, false);
			setLastInputWasSpeech(false);
		} finally {
			setLoading(false);
		}
	};

	const handleInputChange = (value: string) => {
		setChatQuestion(value);

		if (!isListening) {
			setLastInputWasSpeech(false);
		}
	};

	return (
		<div className="chat">
			<ChatTitle isListening={isListening} onMicClick={handleMicClick} />
			<MessagesDisplay messages={messages} isLoading={loading} />
			<MessageInput
				value={chatQuestion}
				onChange={handleInputChange}
				onSubmit={handleSubmitQuestion}
				isLoading={loading}
			/>
		</div>
	);
}
export default ResumeChat;