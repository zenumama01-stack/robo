__all__ = ["ImageGenCompletedEvent", "Usage", "UsageInputTokensDetails"]
class ImageGenCompletedEvent(BaseModel):
    """Emitted when image generation has completed and the final image is available."""
    """Base64-encoded image data, suitable for rendering as an image."""
    """The background setting for the generated image."""
    """The output format for the generated image."""
    """The quality setting for the generated image."""
    """The size of the generated image."""
    type: Literal["image_generation.completed"]
    """The type of the event. Always `image_generation.completed`."""
