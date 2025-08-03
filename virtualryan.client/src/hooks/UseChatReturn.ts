import type * as React from 'react';
import type { Pronunciation } from '../pronunciation';
import type { TextToSpeech } from '../textToSpeech';
import type { ChatMessage } from '../types/chat';

export interface UseChatReturn {
    messages: ChatMessage[];
    setMessages: React.Dispatch<React.SetStateAction<ChatMessage[]>>;
    addChatMessage: (content: string, isUser: boolean) => ChatMessage;
    speakBotResponse: (text: string) => void;
    textToSpeechRef: React.MutableRefObject<TextToSpeech | null>;
    pronunciationRef: React.MutableRefObject<Pronunciation | null>;
}
