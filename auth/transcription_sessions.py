from ....types.beta.realtime import transcription_session_create_params
from ....types.beta.realtime.transcription_session import TranscriptionSession
__all__ = ["TranscriptionSessions", "AsyncTranscriptionSessions"]
class TranscriptionSessions(SyncAPIResource):
    def with_raw_response(self) -> TranscriptionSessionsWithRawResponse:
        return TranscriptionSessionsWithRawResponse(self)
    def with_streaming_response(self) -> TranscriptionSessionsWithStreamingResponse:
        return TranscriptionSessionsWithStreamingResponse(self)
        client_secret: transcription_session_create_params.ClientSecret | NotGiven = NOT_GIVEN,
        include: List[str] | NotGiven = NOT_GIVEN,
        input_audio_noise_reduction: transcription_session_create_params.InputAudioNoiseReduction
        input_audio_transcription: transcription_session_create_params.InputAudioTranscription | NotGiven = NOT_GIVEN,
        turn_detection: transcription_session_create_params.TurnDetection | NotGiven = NOT_GIVEN,
    ) -> TranscriptionSession:
        Realtime API specifically for realtime transcriptions. Can be configured with
        the same session parameters as the `transcription_session.update` client event.
          include:
              The set of items to include in the transcription. Current available items are:
              - `item.input_audio_transcription.logprobs`
          input_audio_transcription: Configuration for input audio transcription. The client can optionally set the
              language and prompt for transcription, these offer additional guidance to the
              transcription service.
            "/realtime/transcription_sessions",
                transcription_session_create_params.TranscriptionSessionCreateParams,
            cast_to=TranscriptionSession,
class AsyncTranscriptionSessions(AsyncAPIResource):
    def with_raw_response(self) -> AsyncTranscriptionSessionsWithRawResponse:
        return AsyncTranscriptionSessionsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncTranscriptionSessionsWithStreamingResponse:
        return AsyncTranscriptionSessionsWithStreamingResponse(self)
class TranscriptionSessionsWithRawResponse:
    def __init__(self, transcription_sessions: TranscriptionSessions) -> None:
        self._transcription_sessions = transcription_sessions
            transcription_sessions.create,
class AsyncTranscriptionSessionsWithRawResponse:
    def __init__(self, transcription_sessions: AsyncTranscriptionSessions) -> None:
class TranscriptionSessionsWithStreamingResponse:
class AsyncTranscriptionSessionsWithStreamingResponse:
