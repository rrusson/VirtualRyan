// Provides basic text-to-speech functionality using the Web Speech API.
export interface TextToSpeechOptions {
	lang?: string;
	rate?: number;
	pitch?: number;
	volume?: number;
	voice?: SpeechSynthesisVoice;
}

export class TextToSpeech {
	private _synth: SpeechSynthesis | null = null;
	private _utterance: SpeechSynthesisUtterance | null = null;
	private _selectedVoice: SpeechSynthesisVoice | null = null;
	private _voicesLoaded: boolean = false;

	constructor() {
		this._synth = window.speechSynthesis || null;
		this.initializeVoices();
	}

	private detectBrowser(): 'edge' | 'chrome' | 'other' {
		const userAgent = navigator.userAgent.toLowerCase();

		// Edge detection (must come before Chrome since Edge contains "chrome" in userAgent)
		if (userAgent.includes('edg/')) {
			return 'edge';
		}

		// Chrome detection
		if (userAgent.includes('chrome/') && !userAgent.includes('edg/')) {
			return 'chrome';
		}

		return 'other';
	}

	private initializeVoices(): void {
		if (!this._synth) {
			return;
		}

		const loadVoices = () => {
			const voices = this._synth!.getVoices();
			if (voices.length > 0) {
				this._voicesLoaded = true;
				this._selectedVoice = this.selectBestVoice();
				console.log('Selected voice:', this._selectedVoice?.name, this._selectedVoice?.lang);
			}
		};

		loadVoices();

		// Also listen for the voiceschanged event (some browsers load voices asynchronously)
		if (!this._voicesLoaded) {
			this._synth.addEventListener('voiceschanged', loadVoices);
		}
	}

	private selectBestVoice(): SpeechSynthesisVoice | null {
		const voices = this.getVoices();
		if (voices.length === 0) {
			return null;
		}

		const browser = this.detectBrowser();
		console.log('Detected browser:', browser);
		console.log('Available voices:', voices.map(v => ({ name: v.name, lang: v.lang, localService: v.localService })));

		let targetVoice: SpeechSynthesisVoice | undefined;

		switch (browser) {
			case 'edge':
				targetVoice = voices.find(voice => voice.name === "Microsoft Ryan Online (Natural) - English (United Kingdom)");
				if (targetVoice) {
					console.log('Found Edge preferred voice:', targetVoice.name);
					return targetVoice;
				}
				break;

			case 'chrome':
				targetVoice = voices.find(voice => voice.name === "Google UK English Male");
				if (targetVoice) {
					console.log('Found Chrome preferred voice:', targetVoice.name);
					return targetVoice;
				}
				break;

			case 'other':
				// For other browsers, continue with fallback logic below
				break;
		}

		// Fallback for all browsers: en-GB or en-US voices without "female"
		const englishVoices = voices.filter(voice =>
			voice.lang === "en-GB" || voice.lang === "en-US"
		);

		if (englishVoices.length === 0) {
			console.log('No en-GB or en-US voices found, using first available voice');
			return voices[0];
		}

		// Filter out voices that contain "female" in the name (case insensitive)
		const nonFemaleVoices = englishVoices.filter(voice =>
			!voice.name.toLowerCase().includes('female')
		);

		const voicesToConsider = nonFemaleVoices.length > 0 ? nonFemaleVoices : englishVoices;

		// Sort UK first, then US, and take the first one
		voicesToConsider.sort((a, b) => a.lang.localeCompare(b.lang));

		console.log('Fallback voice selection from:', voicesToConsider.map(v => ({ name: v.name, lang: v.lang })));
		console.log('Selected fallback voice:', voicesToConsider[0].name, voicesToConsider[0].lang);

		return voicesToConsider[0];
	}

	speak(text: string, options: TextToSpeechOptions = {}): void {
		if (!this._synth) {
			throw new Error('Speech Synthesis not supported in this browser.');
		}

		if (this._synth.speaking) {
			this._synth.cancel();
		}

		this._utterance = new SpeechSynthesisUtterance(text);

		// Use the selected voice if no voice is specified in options
		if (!options.voice && this._selectedVoice) {
			this._utterance.voice = this._selectedVoice;
		} else if (options.voice) {
			this._utterance.voice = options.voice;
		}

		if (options.lang) {
			this._utterance.lang = options.lang;
		}
		if (options.rate) {
			this._utterance.rate = options.rate;
		}
		if (options.pitch) {
			this._utterance.pitch = options.pitch;
		}
		if (options.volume) {
			this._utterance.volume = options.volume;
		}

		this._synth.speak(this._utterance);
	}

	stop(): void {
		if (this._synth && this._synth.speaking) {
			this._synth.cancel();
		}
	}

	pause(): void {
		if (this._synth && this._synth.speaking) {
			this._synth.pause();
		}
	}

	resume(): void {
		if (this._synth && this._synth.paused) {
			this._synth.resume();
		}
	}

	getVoices(): SpeechSynthesisVoice[] {
		if (!this._synth) {
			return [];
		}
		return this._synth.getVoices();
	}

	getSelectedVoice(): SpeechSynthesisVoice | null {
		return this._selectedVoice;
	}

	// Method to manually select a different voice
	selectVoice(voice: SpeechSynthesisVoice): void {
		this._selectedVoice = voice;
		console.log('Manually selected voice:', voice.name, voice.lang);
	}

	// Method to get the detected browser
	getBrowser(): string {
		return this.detectBrowser();
	}
}

export default TextToSpeech;