__all__ = ["ResponseInputImage"]
class ResponseInputImage(BaseModel):
    """An image input to the model.
    Learn about [image inputs](https://platform.openai.com/docs/guides/vision).
    """The type of the input item. Always `input_image`."""
    """The URL of the image to be sent to the model.
    A fully qualified URL or base64 encoded image in a data URL.
    detail: Literal["low", "high", "auto"]
