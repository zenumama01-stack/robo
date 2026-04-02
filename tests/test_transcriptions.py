from openai.types.audio import TranscriptionCreateResponse
class TestTranscriptions:
        transcription = client.audio.transcriptions.create(
            model="gpt-4o-transcribe",
        assert_matches_type(TranscriptionCreateResponse, transcription, path=["response"])
            chunking_strategy="auto",
            include=["logprobs"],
            known_speaker_names=["string"],
            known_speaker_references=["string"],
            language="language",
            prompt="prompt",
            response_format="json",
            timestamp_granularities=["word"],
        response = client.audio.transcriptions.with_raw_response.create(
        transcription = response.parse()
        with client.audio.transcriptions.with_streaming_response.create(
        transcription_stream = client.audio.transcriptions.create(
        transcription_stream.response.close()
class TestAsyncTranscriptions:
        transcription = await async_client.audio.transcriptions.create(
        response = await async_client.audio.transcriptions.with_raw_response.create(
        async with async_client.audio.transcriptions.with_streaming_response.create(
            transcription = await response.parse()
        transcription_stream = await async_client.audio.transcriptions.create(
        await transcription_stream.response.aclose()
