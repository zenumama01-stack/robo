__all__ = ["ResponseImageGenCallInProgressEvent"]
class ResponseImageGenCallInProgressEvent(BaseModel):
    """Emitted when an image generation tool call is in progress."""
    type: Literal["response.image_generation_call.in_progress"]
    """The type of the event. Always 'response.image_generation_call.in_progress'."""
