__all__ = ["ResponseImageGenCallGeneratingEvent"]
class ResponseImageGenCallGeneratingEvent(BaseModel):
    Emitted when an image generation tool call is actively generating an image (intermediate state).
    """The sequence number of the image generation item being processed."""
    type: Literal["response.image_generation_call.generating"]
    """The type of the event. Always 'response.image_generation_call.generating'."""
