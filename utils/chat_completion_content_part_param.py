from .chat_completion_content_part_image_param import ChatCompletionContentPartImageParam
from .chat_completion_content_part_input_audio_param import ChatCompletionContentPartInputAudioParam
__all__ = ["ChatCompletionContentPartParam", "File", "FileFile"]
class FileFile(TypedDict, total=False):
    file_data: str
    The base64 encoded file data, used when passing the file to the model as a
    """The ID of an uploaded file to use as input."""
    """The name of the file, used when passing the file to the model as a string."""
class File(TypedDict, total=False):
    Learn about [file inputs](https://platform.openai.com/docs/guides/text) for text generation.
    file: Required[FileFile]
    type: Required[Literal["file"]]
    """The type of the content part. Always `file`."""
ChatCompletionContentPartParam: TypeAlias = Union[
    ChatCompletionContentPartTextParam,
    ChatCompletionContentPartImageParam,
    ChatCompletionContentPartInputAudioParam,
