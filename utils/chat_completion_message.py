from .chat_completion_audio import ChatCompletionAudio
from .chat_completion_message_tool_call import ChatCompletionMessageToolCallUnion
__all__ = ["ChatCompletionMessage", "Annotation", "AnnotationURLCitation", "FunctionCall"]
class AnnotationURLCitation(BaseModel):
    """A URL citation when using web search."""
    """The index of the last character of the URL citation in the message."""
    """The index of the first character of the URL citation in the message."""
    title: str
    """The title of the web resource."""
    """The URL of the web resource."""
class Annotation(BaseModel):
    type: Literal["url_citation"]
    """The type of the URL citation. Always `url_citation`."""
    url_citation: AnnotationURLCitation
class FunctionCall(BaseModel):
class ChatCompletionMessage(BaseModel):
    """The contents of the message."""
    role: Literal["assistant"]
    annotations: Optional[List[Annotation]] = None
    Annotations for the message, when applicable, as when using the
    audio: Optional[ChatCompletionAudio] = None
    If the audio output modality is requested, this object contains data about the
    audio response from the model.
    function_call: Optional[FunctionCall] = None
    tool_calls: Optional[List[ChatCompletionMessageToolCallUnion]] = None
