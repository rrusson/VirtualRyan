import type { PersonConfig } from '../personConfig';
import type { ChatMessage } from './chat';

// Props for ChatTitle component
export interface ChatTitleProps {
	isListening: boolean;
	onMicClick: () => void;
	personConfig: PersonConfig;
}

// Props for Message component
export interface MessageProps {
	message: ChatMessage;
	showTimestamp: boolean;
	personConfig: PersonConfig;
}

// Props for LoadingMessage component
export interface LoadingMessageProps {
	personConfig: PersonConfig;
}

// Props for MessagesDisplay component
export interface MessagesDisplayProps {
	messages: ChatMessage[];
	isLoading: boolean;
	personConfig: PersonConfig;
}

// Props for MessageInput component
export interface MessageInputProps {
	value: string;
	onChange: (value: string) => void;
	onSubmit: (message: string) => void;
	isLoading: boolean;
	personConfig: PersonConfig;
}

// Re-export ChatMessage for convenience
export type { ChatMessage } from './chat';
