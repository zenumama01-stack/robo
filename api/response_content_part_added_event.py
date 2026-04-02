__all__ = ["ResponseContentPartAddedEvent", "Part"]
class Part(BaseModel):
    """Base64-encoded audio data (if type is "audio")."""
    """The text content (if type is "text")."""
    """The transcript of the audio (if type is "audio")."""
    type: Optional[Literal["text", "audio"]] = None
    """The content type ("text", "audio")."""
class ResponseContentPartAddedEvent(BaseModel):
    """The ID of the item to which the content part was added."""
    part: Part
    """The content part that was added."""
    type: Literal["response.content_part.added"]
    """The event type, must be `response.content_part.added`."""
    Returned when a new content part is added to an assistant message item during
    response generation.
__all__ = ["ResponseContentPartAddedEvent", "Part", "PartReasoningText"]
class PartReasoningText(BaseModel):
Part: TypeAlias = Annotated[
    Union[ResponseOutputText, ResponseOutputRefusal, PartReasoningText], PropertyInfo(discriminator="type")
    """Emitted when a new content part is added."""
    """The index of the content part that was added."""
    """The ID of the output item that the content part was added to."""
    """The index of the output item that the content part was added to."""
    """The type of the event. Always `response.content_part.added`."""
