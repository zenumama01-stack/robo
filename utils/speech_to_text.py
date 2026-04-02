from openai.helpers import Microphone
openai = AsyncOpenAI()
    print("Recording for the next 10 seconds...")
    recording = await Microphone(timeout=10).record()
    print("Recording complete")
    transcription = await openai.audio.transcriptions.create(
        file=recording,
