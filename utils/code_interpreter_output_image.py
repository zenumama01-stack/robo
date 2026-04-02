__all__ = ["CodeInterpreterOutputImage", "Image"]
    The [file](https://platform.openai.com/docs/api-reference/files) ID of the
    image.
class CodeInterpreterOutputImage(BaseModel):
    type: Literal["image"]
    """Always `image`."""
    image: Optional[Image] = None
