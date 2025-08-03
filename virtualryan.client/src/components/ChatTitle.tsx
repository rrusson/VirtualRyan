import * as React from 'react';
import type { ChatTitleProps } from '../types/interfaces';

export const ChatTitle: React.FC<ChatTitleProps> = ({ isListening, onMicClick, personConfig }) => {
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