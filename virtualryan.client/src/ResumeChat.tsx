import * as React from 'react';
import { useState, useRef, useEffect } from 'react';
import { SpeechToText } from './speechToText';
import { type PersonConfig, defaultPersonConfig } from './personConfig';
import { useChat } from './hooks/useChat';
import TextToSpeech from './textToSpeech';
import { Pronunciation } from './pronunciation';
import type {
	ChatTitleProps,
	ChatMessage,
	MessageProps,
	LoadingMessageProps,
	MessagesDisplayProps,
	MessageInputProps
} from './types/interfaces';

// ChatTitle Component
const ChatTitle: React.FC<ChatTitleProps> = ({ isListening, onMicClick, personConfig }) => {
	return (
		<div className="chat-title">
			<h1>Resume Bot</h1>
			<h2>{personConfig.subtitle}</h2>
			<figure className="avatar">
				<img src={personConfig.avatarUrl} alt="avatar" />
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

// Message Component
const Message: React.FC<MessageProps> = ({ message, showTimestamp, personConfig }) => {
	const formatTime = (date: Date) => {
		return String(date.getHours()).padStart(2, '0') + ':' + String(date.getMinutes()).padStart(2, '0');
	};

	return (
		<div className={`message ${message.isUser ? 'message-personal ' : ''}new`} data-id={message.id}>
			{!message.isUser && (
				<figure className="avatar">
					<img src={personConfig.avatarUrl} alt="avatar" />
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
const LoadingMessage: React.FC<LoadingMessageProps> = ({ personConfig }) => {
	return (
		<div className="message loading new">
			<figure className="avatar">
				<img src={personConfig.avatarUrl} alt="avatar" />
			</figure>
			<span></span>
		</div>
	);
};

// Messages Display Component
const MessagesDisplay: React.FC<MessagesDisplayProps> = ({ messages, isLoading, personConfig }) => {
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
						<Message key={message.id} message={message} showTimestamp={showTimestamp} personConfig={personConfig} />
					))}
					{isLoading && <LoadingMessage personConfig={personConfig} />}
					<div ref={messagesEndRef} />
				</div>
			</div>
		</div>
	);
};

// Message Input Component
const MessageInput: React.FC<MessageInputProps> = ({ value, onChange, onSubmit, isLoading, personConfig }) => {
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
					placeholder={`Ask a question about ${personConfig.firstName}'s resume...`}
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
	const [loading, setLoading] = useState<boolean>(false);
	const [isListening, setIsListening] = useState<boolean>(false);
	const [personConfig] = useState<PersonConfig>(defaultPersonConfig);

	const speechToTextRef = useRef<SpeechToText | null>(null);
	const {
		messages,
		setMessages,
		addChatMessage,
		speakBotResponse,
		textToSpeechRef,
		pronunciationRef
	} = useChat();

	// Initialize TTS and Pronunciation instance, and add welcome message
	useEffect(() => {
		if (!textToSpeechRef.current) textToSpeechRef.current = new TextToSpeech();
		if (!pronunciationRef.current) pronunciationRef.current = new Pronunciation();

		const welcomeMessage: ChatMessage = {
			id: 'welcome-' + Date.now(),
			content: personConfig.welcomeMsg,
			isUser: false,
			timestamp: new Date()
		};
		setMessages([welcomeMessage]);
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [personConfig.welcomeMsg]);

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
				setIsListening(true);
			};

			stt.onEnd = () => {
				setIsListening(false);
			};

			stt.onError = (error: string) => {
				setIsListening(false);
				addChatMessage(`Speech recognition error: ${error}`, false);
			};

			stt.onResult = (transcript: string) => {
				if (transcript && transcript.trim()) {
					setChatQuestion(transcript.trim());
					setTimeout(() => {
						textToSpeechRef.current?.stop();
						handleSubmitQuestion(transcript.trim(), true);
					}, 100);
				}
			};

			stt.listen();

		} catch (error) {
			setIsListening(false);
			addChatMessage('Speech recognition is not supported in this browser or an error occurred.', false);
		}
	};

	const handleSubmitQuestion = async (questionToSubmit?: string, wasInputSpeech: boolean = false) => {
		const question = questionToSubmit || chatQuestion;

		if (!question.trim()) {
			return;
		}

		addChatMessage(question, true);
		setLoading(true);
		setChatQuestion('');

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
				}
			} else {
				const errorText = await response.text();
				addChatMessage('Error getting response: ' + errorText, false);
			}
		} catch (error) {
			addChatMessage('Error getting response: ' + error, false);
		} finally {
			setLoading(false);
		}
	};

	const handleInputChange = (value: string) => {
		setChatQuestion(value);
	};

	return (
		<div className="chat">
			<ChatTitle isListening={isListening} onMicClick={handleMicClick} personConfig={personConfig} />
			<MessagesDisplay messages={messages} isLoading={loading} personConfig={personConfig} />
			<MessageInput
				value={chatQuestion}
				onChange={handleInputChange}
				onSubmit={handleSubmitQuestion}
				isLoading={loading}
				personConfig={personConfig}
			/>
		</div>
	);
}
export default ResumeChat;