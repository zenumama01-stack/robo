from .image_file_param import ImageFileParam
__all__ = ["ImageFileContentBlockParam"]
class ImageFileContentBlockParam(TypedDict, total=False):
    image_file: Required[ImageFileParam]
    type: Required[Literal["image_file"]]
