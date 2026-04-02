from typing import Union
from ... import _legacy_response
from ..._types import Body, Omit, Query, Headers, NotGiven, omit, not_given
from ..._utils import maybe_transform, async_maybe_transform
from ..._response import (
from ...types.audio import speech_create_params
from ..._base_client import make_request_options
from ...types.audio.speech_model import SpeechModel
__all__ = ["Speech", "AsyncSpeech"]
class Speech(SyncAPIResource):
    def with_raw_response(self) -> SpeechWithRawResponse:
        return SpeechWithRawResponse(self)
    def with_streaming_response(self) -> SpeechWithStreamingResponse:
        return SpeechWithStreamingResponse(self)
        input: str,
        model: Union[str, SpeechModel],
        voice: speech_create_params.Voice,
        instructions: str | Omit = omit,
        response_format: Literal["mp3", "opus", "aac", "flac", "wav", "pcm"] | Omit = omit,
        speed: float | Omit = omit,
        stream_format: Literal["sse", "audio"] | Omit = omit,
        Generates audio from the input text.
        Returns the audio file content, or a stream of audio events.
          input: The text to generate audio for. The maximum length is 4096 characters.
          model:
              One of the available [TTS models](https://platform.openai.com/docs/models#tts):
              `tts-1`, `tts-1-hd`, `gpt-4o-mini-tts`, or `gpt-4o-mini-tts-2025-12-15`.
          voice: The voice to use when generating the audio. Supported built-in voices are
              `alloy`, `ash`, `ballad`, `coral`, `echo`, `fable`, `onyx`, `nova`, `sage`,
              `shimmer`, `verse`, `marin`, and `cedar`. You may also provide a custom voice
              object with an `id`, for example `{ "id": "voice_1234" }`. Previews of the
              voices are available in the
              [Text to speech guide](https://platform.openai.com/docs/guides/text-to-speech#voice-options).
          instructions: Control the voice of your generated audio with additional instructions. Does not
              work with `tts-1` or `tts-1-hd`.
          response_format: The format to audio in. Supported formats are `mp3`, `opus`, `aac`, `flac`,
              `wav`, and `pcm`.
          speed: The speed of the generated audio. Select a value from `0.25` to `4.0`. `1.0` is
              the default.
          stream_format: The format to stream the audio in. Supported formats are `sse` and `audio`.
              `sse` is not supported for `tts-1` or `tts-1-hd`.
        extra_headers = {"Accept": "application/octet-stream", **(extra_headers or {})}
                    "voice": voice,
                    "instructions": instructions,
                    "speed": speed,
                    "stream_format": stream_format,
                speech_create_params.SpeechCreateParams,
class AsyncSpeech(AsyncAPIResource):
    def with_raw_response(self) -> AsyncSpeechWithRawResponse:
        return AsyncSpeechWithRawResponse(self)
    def with_streaming_response(self) -> AsyncSpeechWithStreamingResponse:
        return AsyncSpeechWithStreamingResponse(self)
class SpeechWithRawResponse:
    def __init__(self, speech: Speech) -> None:
        self._speech = speech
            speech.create,
class AsyncSpeechWithRawResponse:
    def __init__(self, speech: AsyncSpeech) -> None:
class SpeechWithStreamingResponse:
        self.create = to_custom_streamed_response_wrapper(
class AsyncSpeechWithStreamingResponse:
        self.create = async_to_custom_streamed_response_wrapper(
            str, Literal["alloy", "ash", "ballad", "coral", "echo", "sage", "shimmer", "verse", "marin", "cedar"]
              `shimmer`, `verse`, `marin`, and `cedar`. Previews of the voices are available
