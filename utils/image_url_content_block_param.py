from .image_url_param import ImageURLParam
__all__ = ["ImageURLContentBlockParam"]
class ImageURLContentBlockParam(TypedDict, total=False):
    image_url: Required[ImageURLParam]
