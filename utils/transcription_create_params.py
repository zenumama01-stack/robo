from ..._types import FileTypes, SequenceNotStr
from ..audio_model import AudioModel
from .transcription_include import TranscriptionInclude
from ..audio_response_format import AudioResponseFormat
    "TranscriptionCreateParamsBase",
    "ChunkingStrategy",
    "ChunkingStrategyVadConfig",
    "TranscriptionCreateParamsNonStreaming",
    "TranscriptionCreateParamsStreaming",
class TranscriptionCreateParamsBase(TypedDict, total=False):
    model: Required[Union[str, AudioModel]]
    The options are `gpt-4o-transcribe`, `gpt-4o-mini-transcribe`,
    `gpt-4o-mini-transcribe-2025-12-15`, `whisper-1` (which is powered by our open
    source Whisper V2 model), and `gpt-4o-transcribe-diarize`.
    chunking_strategy: Optional[ChunkingStrategy]
    """Controls how the audio is cut into chunks.
    When set to `"auto"`, the server first normalizes loudness and then uses voice
    activity detection (VAD) to choose boundaries. `server_vad` object can be
    provided to tweak VAD detection parameters manually. If unset, the audio is
    transcribed as a single block. Required when using `gpt-4o-transcribe-diarize`
    for inputs longer than 30 seconds.
    include: List[TranscriptionInclude]
    Additional information to include in the transcription response. `logprobs` will
    known_speaker_names: SequenceNotStr[str]
    Optional list of speaker names that correspond to the audio samples provided in
    known_speaker_references: SequenceNotStr[str]
    Optional list of audio samples (as
    language: str
    """The language of the input audio.
    Supplying the input language in
    """An optional text to guide the model's style or continue a previous audio
    segment.
    The [prompt](https://platform.openai.com/docs/guides/speech-to-text#prompting)
    response_format: AudioResponseFormat
    The format of the output, in one of these options: `json`, `text`, `srt`,
    temperature: float
    """The sampling temperature, between 0 and 1.
    0.2 will make it more focused and deterministic. If set to 0, the model will use
    timestamp_granularities: List[Literal["word", "segment"]]
    """The timestamp granularities to populate for this transcription.
class ChunkingStrategyVadConfig(TypedDict, total=False):
    type: Required[Literal["server_vad"]]
    """Must be set to `server_vad` to enable manual chunking using server side VAD."""
    prefix_padding_ms: int
    """Amount of audio to include before the VAD detected speech (in milliseconds)."""
    silence_duration_ms: int
    Duration of silence to detect speech stop (in milliseconds). With shorter values
    the model will respond more quickly, but may jump in on short pauses from the
    threshold: float
    """Sensitivity threshold (0.0 to 1.0) for voice activity detection.
    A higher threshold will require louder audio to activate the model, and thus
    might perform better in noisy environments.
ChunkingStrategy: TypeAlias = Union[Literal["auto"], ChunkingStrategyVadConfig]
class TranscriptionCreateParamsNonStreaming(TranscriptionCreateParamsBase, total=False):
    If set to true, the model response data will be streamed to the client as it is
class TranscriptionCreateParamsStreaming(TranscriptionCreateParamsBase):
TranscriptionCreateParams = Union[TranscriptionCreateParamsNonStreaming, TranscriptionCreateParamsStreaming]
