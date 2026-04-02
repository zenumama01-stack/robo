from .text_content_block import TextContentBlock
from .refusal_content_block import RefusalContentBlock
from .image_url_content_block import ImageURLContentBlock
from .image_file_content_block import ImageFileContentBlock
__all__ = ["MessageContent"]
MessageContent: TypeAlias = Annotated[
    Union[ImageFileContentBlock, ImageURLContentBlock, TextContentBlock, RefusalContentBlock],
