import * as React from 'react';

const renderLineWithLinks = (line: string, messageId: string, lineIndex: number) => {
	const markdownLinkPattern = /\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/g;
	const nodes: React.ReactNode[] = [];
	let currentIndex = 0;
	let match: RegExpExecArray | null;

	while ((match = markdownLinkPattern.exec(line)) !== null) {
		const matchStart = match.index;
		const fullMatch = match[0];
		const title = match[1];
		const url = match[2];

		if (matchStart > currentIndex) {
			nodes.push(line.substring(currentIndex, matchStart));
		}

		nodes.push(
			<a key={`${messageId}-link-${lineIndex}-${matchStart}`} href={url} target="_blank" rel="noopener noreferrer">
				{title}
			</a>
		);

		currentIndex = matchStart + fullMatch.length;
	}

	if (currentIndex < line.length) {
		nodes.push(line.substring(currentIndex));
	}

	if (nodes.length === 0) {
		nodes.push(line);
	}

	return nodes;
};

export const renderMessageContent = (content: string, messageId: string) => {
	const lines = content.split(/\r?\n/);
	return lines.map((line, index) => (
		<React.Fragment key={`${messageId}-line-${index}`}>
			{renderLineWithLinks(line, messageId, index)}
			{index < lines.length - 1 && <br />}
		</React.Fragment>
	));
};
