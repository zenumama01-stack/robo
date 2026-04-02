from .image_url import ImageURL
__all__ = ["ImageURLContentBlock"]
class ImageURLContentBlock(BaseModel):
    """References an image URL in the content of a message."""
    image_url: ImageURL
    type: Literal["image_url"]
    """The type of the content part."""
