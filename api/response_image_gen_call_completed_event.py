__all__ = ["ResponseImageGenCallCompletedEvent"]
class ResponseImageGenCallCompletedEvent(BaseModel):
    Emitted when an image generation tool call has completed and the final image is available.
    """The unique identifier of the image generation item being processed."""
    """The index of the output item in the response's output array."""
    type: Literal["response.image_generation_call.completed"]
    """The type of the event. Always 'response.image_generation_call.completed'."""
