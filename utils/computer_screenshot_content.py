__all__ = ["ComputerScreenshotContent"]
class ComputerScreenshotContent(BaseModel):
    """A screenshot of a computer."""
    detail: Literal["low", "high", "auto", "original"]
    """The detail level of the screenshot image to be sent to the model.
    One of `high`, `low`, `auto`, or `original`. Defaults to `auto`.
    """The identifier of an uploaded file that contains the screenshot."""
    image_url: Optional[str] = None
    """The URL of the screenshot image."""
    type: Literal["computer_screenshot"]
    """Specifies the event type.
    For a computer screenshot, this property is always set to `computer_screenshot`.
