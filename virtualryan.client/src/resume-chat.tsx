import * as React from 'react';
import { useState, useRef, useEffect } from 'react';
import './resume-chat.css';
import { SpeechToText } from './speechToText';
import { TextToSpeech } from './textToSpeech';
import { Pronunciation } from './pronunciation';

// Type definitions for jQuery and global functions
interface WindowWithJQuery extends Window {
	$?: any; // eslint-disable-line @typescript-eslint/no-explicit-any
	insertUserMessage?: (message: string) => void;
	addBotResponse?: (response: string) => void;
	showBotLoading?: () => void;
	hideBotLoading?: () => void;
}

function ResumeChat() {
	const [chatQuestion, setChatQuestion] = useState<string>('');
	const [loading, setLoading] = useState<boolean>(false);
	const [isListening, setIsListening] = useState<boolean>(false);
	const [wasLastInputSpeech, setLastInputWasSpeech] = useState<boolean>(false);
	const formRef = useRef<HTMLFormElement>(null);
	const micButtonRef = useRef<HTMLButtonElement>(null);
	const speechToTextRef = useRef<SpeechToText | null>(null);
	const textToSpeechRef = useRef<TextToSpeech | null>(null);
	const pronunciationRef = useRef<Pronunciation | null>(null);

	// Initialize TTS and Pronunciation instance
	useEffect(() => {
		textToSpeechRef.current = new TextToSpeech();
		pronunciationRef.current = new Pronunciation();

		// Log the selected voice after a short delay to allow voice loading
		setTimeout(() => {
			const selectedVoice = textToSpeechRef.current?.getSelectedVoice();
			if (selectedVoice) {
				console.log('TextToSpeech initialized with voice:', selectedVoice.name, selectedVoice.lang);
			} else {
				console.log('TextToSpeech initialized but no voice selected yet');
			}
		}, 1000);
	}, []);

	// Initialize the jQuery scrollbar when component mounts
	useEffect(() => {
		// Ensure jQuery and chatScript functions are available
		const checkAndInit = () => {
			const windowWithJQuery = window as unknown as WindowWithJQuery;
			if (typeof window !== 'undefined' && windowWithJQuery.$ && typeof windowWithJQuery.insertUserMessage === 'function') {
				const $ = windowWithJQuery.$;
				const $messages = $('.messages-content');
				if ($messages.length > 0 && typeof $messages.mCustomScrollbar === 'function') {
					$messages.mCustomScrollbar();
				}
			} else {
				setTimeout(checkAndInit, 100);
			}
		};
		checkAndInit();
	}, []);

	// Function to speak bot response
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
			// Stop listening
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

				if (micButtonRef.current) {
					micButtonRef.current.style.color = '#ff4444';
				}
			};

			stt.onEnd = () => {
				console.log('Speech recognition ended');
				setIsListening(false);

				if (micButtonRef.current) {
					micButtonRef.current.style.color = '';
				}
			};

			stt.onError = (error: string) => {
				console.error('Speech recognition error:', error);
				setIsListening(false);
				setLastInputWasSpeech(false);

				if (micButtonRef.current) {
					micButtonRef.current.style.color = '';
				}

				// Show error to user
				const windowWithJQuery = window as unknown as WindowWithJQuery;
				if (typeof windowWithJQuery.addBotResponse === 'function') {
					windowWithJQuery.addBotResponse(`Speech recognition error: ${error}`);
				}
			};

			stt.onResult = (transcript: string) => {
				console.log('Speech transcript received:', transcript);

				if (transcript && transcript.trim()) {
					// Set flag that last input was speech
					setLastInputWasSpeech(true);

					// Set the transcript in the input field
					setChatQuestion(transcript.trim());

					// Auto-submit the form after a short delay
					setTimeout(() => {
						if (formRef.current) {
							textToSpeechRef.current?.stop();
							formRef.current.requestSubmit();
						}
					}, 100);
				}
			};

			// Start listening
			stt.listen();

		} catch (error) {
			console.error('Error initializing speech recognition:', error);
			setIsListening(false);
			setLastInputWasSpeech(false);

			// Show error to user
			const windowWithJQuery = window as unknown as WindowWithJQuery;
			if (typeof windowWithJQuery.addBotResponse === 'function') {
				windowWithJQuery.addBotResponse('Speech recognition is not supported in this browser or an error occurred.');
			}
		}
	};

	const submitQuestion = async (event: React.FormEvent<HTMLFormElement>) => {
		event.preventDefault();

		if (!chatQuestion.trim()) {
			console.log('No question entered.');
			return;
		}

		// Insert user message using jQuery function
		const windowWithJQuery = window as unknown as WindowWithJQuery;
		if (typeof windowWithJQuery.insertUserMessage === 'function') {
			windowWithJQuery.insertUserMessage(chatQuestion);
		}

		setLoading(true);
		const questionToSubmit = chatQuestion; // Store the question and speech flag before clearing
		const wasInputSpeech = wasLastInputSpeech;
		setChatQuestion('');

		if (!wasInputSpeech) {
			setLastInputWasSpeech(false);
		}

		if (typeof windowWithJQuery.showBotLoading === 'function') {
			windowWithJQuery.showBotLoading();
		}

		try {
			const response = await fetch('/Chat/AskQuestion', {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({ question: questionToSubmit }),
			});

			if (response.ok) {
				const result = await response.text();

				if (typeof windowWithJQuery.hideBotLoading === 'function') {
					windowWithJQuery.hideBotLoading();
				}
				if (typeof windowWithJQuery.addBotResponse === 'function') {
					windowWithJQuery.addBotResponse(result);
				}

				if (wasInputSpeech) {
					speakBotResponse(result);
					setLastInputWasSpeech(false);
				}
			} else {
				const errorText = await response.text();
				console.error('HTTP error:', errorText);

				// Hide loading and show error using jQuery functions
				if (typeof windowWithJQuery.hideBotLoading === 'function') {
					windowWithJQuery.hideBotLoading();
				}
				if (typeof windowWithJQuery.addBotResponse === 'function') {
					windowWithJQuery.addBotResponse('Error getting response: ' + errorText);
				}

				setLastInputWasSpeech(false);
			}
		} catch (error) {
			console.error('Fetch error:', error);

			// Hide loading and show error using jQuery functions
			if (typeof windowWithJQuery.hideBotLoading === 'function') {
				windowWithJQuery.hideBotLoading();
			}
			if (typeof windowWithJQuery.addBotResponse === 'function') {
				windowWithJQuery.addBotResponse('Error getting response: ' + error);
			}

			setLastInputWasSpeech(false);
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

	// Handle manual input changes (typing) - reset speech flag
	const handleInputChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
		setChatQuestion(e.target.value);
		// If user is typing manually, reset the speech flag
		if (!isListening) {
			setLastInputWasSpeech(false);
		}
	};

	return (
		<div className="chat">
			<div className="chat-title">
				<h1>Resume Bot</h1>
				<h2>Rep. Ryan Russon</h2>
				<figure className="avatar">
					<img src="https://avatars.githubusercontent.com/u/653188?v=4" alt="avatar" />
				</figure>
				<button
					ref={micButtonRef}
					className="mic-button"
					aria-label={isListening ? "Stop voice input" : "Start voice input"}
					type="button"
					onClick={handleMicClick}
					style={{
						color: isListening ? '#ff4444' : '',
						animation: isListening ? 'pulse 1s infinite' : 'none'
					}}
				>
					<i className="fa fa-microphone" />
				</button>
			</div>
			<div className="messages">
				<div className="messages-content">
				</div>
			</div>
			<form ref={formRef} onSubmit={submitQuestion} style={{ marginBottom: '0' }}>
				<div className="message-box">
					<textarea
						className="message-input"
						id="chatQuestion"
						value={chatQuestion}
						onChange={handleInputChange}
						onKeyDown={handleKeyDown}
						name="chatQuestion"
						rows={8}
						cols={80}
						placeholder="Ask a question about Ryan's resume..." />
					<button type="submit" className="message-submit" disabled={loading || !chatQuestion.trim()}>Send</button>
				</div>
			</form>
		</div>
	);
}

export default ResumeChat;