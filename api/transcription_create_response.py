from .transcription import Transcription
from .transcription_verbose import TranscriptionVerbose
from .transcription_diarized import TranscriptionDiarized
__all__ = ["TranscriptionCreateResponse"]
TranscriptionCreateResponse: TypeAlias = Union[Transcription, TranscriptionDiarized, TranscriptionVerbose]
