from .image_file_delta import ImageFileDelta
__all__ = ["ImageFileDeltaBlock"]
class ImageFileDeltaBlock(BaseModel):
    """The index of the content part in the message."""
    image_file: Optional[ImageFileDelta] = None
