interface SpeechRecognitionOptions {
  onTranscript: (transcript: string) => void;
  onError?: (error: string) => void;
  continuous?: boolean;
  language?: string;
}

export class SpeechRecognitionService {
  private recognition: any;
  private isListening = false;

  constructor() {
    // Check if browser supports Speech Recognition
    const SpeechRecognition = 
      (window as any).SpeechRecognition || 
      (window as any).webkitSpeechRecognition;
    
    if (!SpeechRecognition) {
      console.warn("Speech Recognition not supported in this browser");
      return;
    }

    this.recognition = new SpeechRecognition();
    this.recognition.continuous = true;
    this.recognition.interimResults = true;
    this.recognition.lang = 'en-US';
  }

  startListening(options: SpeechRecognitionOptions) {
    if (!this.recognition) {
      options.onError?.("Speech Recognition not supported");
      return;
    }

    if (this.isListening) {
      return;
    }

    this.recognition.continuous = options.continuous ?? true;
    this.recognition.lang = options.language ?? 'en-US';

    this.recognition.onresult = (event: any) => {
      let transcript = '';
      for (let i = event.resultIndex; i < event.results.length; i++) {
        transcript += event.results[i][0].transcript;
      }
      options.onTranscript(transcript);
    };

    this.recognition.onerror = (event: any) => {
      options.onError?.(event.error);
    };

    this.recognition.onend = () => {
      this.isListening = false;
    };

    this.recognition.start();
    this.isListening = true;
  }

  stopListening() {
    if (this.recognition && this.isListening) {
      this.recognition.stop();
      this.isListening = false;
    }
  }

  isSupported(): boolean {
    return !!this.recognition;
  }

  getIsListening(): boolean {
    return this.isListening;
  }
}

export const speechRecognition = new SpeechRecognitionService();
