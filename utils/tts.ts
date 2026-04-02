@RegisterClass(BaseAudioGenerator, "OpenAIAudioGenerator")
export class OpenAIAudioGenerator extends BaseAudioGenerator {
        this._openAI = new OpenAI({apiKey: apiKey});
            const audio = await this._openAI.audio.speech.create({
                model: params.model_id || (await this.GetModels())[0].id,
                voice: params.voice || (await this.GetVoices())[0].id,
                instructions: params.instructions || "Speak in a cheerful and positive tone"
            const arrayBuffer = await audio.arrayBuffer();
            const audioBuffer = Buffer.from(arrayBuffer);
            speechResult.content = audioBuffer.toString('base64');
            const errorInfo = ErrorAnalyzer.analyzeError(error, 'OpenAI TTS');
            console.error(`OpenAI TTS error:`, error, errorInfo);
            { id: "alloy", name: "Alloy" },
            { id: "echo", name: "Echo" },
            { id: "fable", name: "Fable" },
            { id: "onyx", name: "Onyx" },
            { id: "nova", name: "Nova" },
            { id: "shimmer", name: "Shimmer" }
                id: "gpt-4o-mini-tts", 
                name: "GPT-4o Mini TTS",
                supportsTextToSpeech: true,
                supportsVoiceConversion: false,
                supportsStyle: false,
                supportsSpeakerBoost: false,
                supportsFineTuning: false
        return ["CreateSpeech", "GetVoices", "GetModels"];
