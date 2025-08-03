import { useState, useRef, useCallback } from 'react';
import { TextToSpeech } from '../textToSpeech';
import { Pronunciation } from '../pronunciation';
import { type ChatMessage } from '../types/chat';
import type { UseChatReturn } from './UseChatReturn';

export const useChat = (): UseChatReturn => {
	const [messages, setMessages] = useState<ChatMessage[]>([]);
	const textToSpeechRef = useRef<TextToSpeech | null>(null);
	const pronunciationRef = useRef<Pronunciation | null>(null);

	const addChatMessage = useCallback((content: string, isUser: boolean): ChatMessage => {
		const message: ChatMessage = {
			id: (isUser ? 'user-' : 'bot-') + Date.now(),
			content,
			isUser,
			timestamp: new Date()
		};
		setMessages(prev => [...prev, message]);
		return message;
	}, []);

	const speakBotResponse = useCallback((text: string) => {
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
	}, []);

	return {
		messages,
		setMessages,
		addChatMessage,
		speakBotResponse,
		textToSpeechRef,
		pronunciationRef
	};
};