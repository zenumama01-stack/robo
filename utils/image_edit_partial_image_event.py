__all__ = ["ImageEditPartialImageEvent"]
class ImageEditPartialImageEvent(BaseModel):
    """Emitted when a partial image is available during image editing streaming."""
    """Base64-encoded partial image data, suitable for rendering as an image."""
    """The background setting for the requested edited image."""
    """The output format for the requested edited image."""
    partial_image_index: int
    """0-based index for the partial image (streaming)."""
    """The quality setting for the requested edited image."""
    """The size of the requested edited image."""
    type: Literal["image_edit.partial_image"]
    """The type of the event. Always `image_edit.partial_image`."""
