__all__ = ["ResponseImageGenCallPartialImageEvent"]
class ResponseImageGenCallPartialImageEvent(BaseModel):
    partial_image_b64: str
    0-based index for the partial image (backend is 1-based, but this is 0-based for
    the user).
    type: Literal["response.image_generation_call.partial_image"]
    """The type of the event. Always 'response.image_generation_call.partial_image'."""
