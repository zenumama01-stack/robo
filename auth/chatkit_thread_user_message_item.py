from .chatkit_attachment import ChatKitAttachment
    "ChatKitThreadUserMessageItem",
    "ContentInputText",
    "ContentQuotedText",
    "InferenceOptions",
    "InferenceOptionsToolChoice",
class ContentInputText(BaseModel):
    """Text block that a user contributed to the thread."""
    """Plain-text content supplied by the user."""
    type: Literal["input_text"]
    """Type discriminator that is always `input_text`."""
class ContentQuotedText(BaseModel):
    """Quoted snippet that the user referenced in their message."""
    """Quoted text content."""
    type: Literal["quoted_text"]
    """Type discriminator that is always `quoted_text`."""
Content: TypeAlias = Annotated[Union[ContentInputText, ContentQuotedText], PropertyInfo(discriminator="type")]
class InferenceOptionsToolChoice(BaseModel):
    """Preferred tool to invoke. Defaults to null when ChatKit should auto-select."""
    """Identifier of the requested tool."""
class InferenceOptions(BaseModel):
    """Inference overrides applied to the message. Defaults to null when unset."""
    """Model name that generated the response.
    Defaults to null when using the session default.
    tool_choice: Optional[InferenceOptionsToolChoice] = None
class ChatKitThreadUserMessageItem(BaseModel):
    """User-authored messages within a thread."""
    attachments: List[ChatKitAttachment]
    """Attachments associated with the user message. Defaults to an empty list."""
    """Ordered content elements supplied by the user."""
    inference_options: Optional[InferenceOptions] = None
    type: Literal["chatkit.user_message"]
