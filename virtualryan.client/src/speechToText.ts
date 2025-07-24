// Provides basic speech-to-text functionality using the Web Speech API.
declare global {
	interface Window {
		SpeechRecognition?: any;
		webkitSpeechRecognition?: any;
	}
}

export class SpeechToText {
	private recognition: any = null;
	public onResult: ((transcript: string) => void) | null = null;
	public onError: ((error: string) => void) | null = null;
	public onStart: (() => void) | null = null;
	public onEnd: (() => void) | null = null;

	constructor() {	}

	listen(): void {
		// Check for browser support
		const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

		navigator.mediaDevices.getUserMedia({
			audio: {
				echoCancellation: true,
				noiseSuppression: true,
				sampleRate: 44100
			}
		}).then(() => {
			console.log('Microphone access granted');
		}).catch(error => {
			console.error('Microphone access denied or failed:', error);
			if (this.onError) {
				this.onError(error);
			}
			return;
		});

		if (!SpeechRecognition) {
			const error = 'Speech recognition not supported in this browser';
			console.error(error);
			if (this.onError) {
				this.onError(error);
			}
			return;
		}

		// Create new recognition instance
		this.recognition = new SpeechRecognition();

		// Configure recognition settings
		this.recognition.lang = 'en-US';
		this.recognition.interimResults = false;
		this.recognition.continuous = false;
		this.recognition.maxAlternatives = 1;

		// Set up event handlers
		this.recognition.onstart = () => {
			console.log('Speech recognition started');
			if (this.onStart) {
				this.onStart();
			}
		};

		this.recognition.onresult = (event: any) => {
			console.log('Speech recognition result received');

			if (event.results && event.results.length > 0) {
				const transcript = event.results[0][0].transcript;
				const confidence = event.results[0][0].confidence;

				console.log('Speech received: ' + transcript);
				console.log('Confidence: ' + confidence);

				if (this.onResult) {
					this.onResult(transcript);
				}
			}
		};

		this.recognition.onerror = (event: any) => {
			console.error('Speech recognition error:', event.error);
			if (this.onError) {
				this.onError(event.error);
			}
		};

		this.recognition.onend = () => {
			console.log('Speech recognition ended');
			if (this.onEnd) {
				this.onEnd();
			}
		};

		// Start recognition
		try {
			this.recognition.start();
		} catch (error) {
			console.error('Error starting speech recognition:', error);
			if (this.onError) {
				this.onError(error instanceof Error ? error.message : String(error));
			}
		}
	}

	stop(): void {
		if (this.recognition) {
			this.recognition.stop();
		}
	}

	abort(): void {
		if (this.recognition) {
			this.recognition.abort();
		}
	}
}

export default SpeechToText;