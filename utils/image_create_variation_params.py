from .image_model import ImageModel
__all__ = ["ImageCreateVariationParams"]
class ImageCreateVariationParams(TypedDict, total=False):
    image: Required[FileTypes]
    """The image to use as the basis for the variation(s).
    Must be a valid PNG file, less than 4MB, and square.
    model: Union[str, ImageModel, None]
    """The model to use for image generation.
    Only `dall-e-2` is supported at this time.
    """The number of images to generate. Must be between 1 and 10."""
    response_format: Optional[Literal["url", "b64_json"]]
    """The format in which the generated images are returned.
    Must be one of `url` or `b64_json`. URLs are only valid for 60 minutes after the
    image has been generated.
    size: Optional[Literal["256x256", "512x512", "1024x1024"]]
    """The size of the generated images.
    Must be one of `256x256`, `512x512`, or `1024x1024`.
