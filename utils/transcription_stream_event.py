from .transcription_text_done_event import TranscriptionTextDoneEvent
from .transcription_text_delta_event import TranscriptionTextDeltaEvent
from .transcription_text_segment_event import TranscriptionTextSegmentEvent
__all__ = ["TranscriptionStreamEvent"]
TranscriptionStreamEvent: TypeAlias = Annotated[
    Union[TranscriptionTextSegmentEvent, TranscriptionTextDeltaEvent, TranscriptionTextDoneEvent],
