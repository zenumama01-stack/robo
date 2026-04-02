from ..._types import FileTypes
__all__ = ["TranslationCreateParams"]
class TranslationCreateParams(TypedDict, total=False):
    The audio file object (not file name) translate, in one of these formats: flac,
    Only `whisper-1` (which is powered by our open source Whisper V2 model) is
    currently available.
    response_format: Literal["json", "text", "srt", "verbose_json", "vtt"]
