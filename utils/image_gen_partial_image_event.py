__all__ = ["ImageGenPartialImageEvent"]
class ImageGenPartialImageEvent(BaseModel):
    """Emitted when a partial image is available during image generation streaming."""
    """The background setting for the requested image."""
    """The output format for the requested image."""
    """The quality setting for the requested image."""
    """The size of the requested image."""
    type: Literal["image_generation.partial_image"]
    """The type of the event. Always `image_generation.partial_image`."""
