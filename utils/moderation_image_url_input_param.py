__all__ = ["ModerationImageURLInputParam", "ImageURL"]
class ImageURL(TypedDict, total=False):
    """Contains either an image URL or a data URL for a base64 encoded image."""
    """Either a URL of the image or the base64 encoded image data."""
class ModerationImageURLInputParam(TypedDict, total=False):
    """An object describing an image to classify."""
    image_url: Required[ImageURL]
    type: Required[Literal["image_url"]]
    """Always `image_url`."""
