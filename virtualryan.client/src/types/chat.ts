// Types for chat functionality
export interface ChatMessage {
	id: string;
	content: string;
	isUser: boolean;
	timestamp: Date;
}