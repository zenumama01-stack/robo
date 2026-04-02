from .text_delta_block import TextDeltaBlock
from .refusal_delta_block import RefusalDeltaBlock
from .image_url_delta_block import ImageURLDeltaBlock
from .image_file_delta_block import ImageFileDeltaBlock
__all__ = ["MessageContentDelta"]
MessageContentDelta: TypeAlias = Annotated[
    Union[ImageFileDeltaBlock, TextDeltaBlock, RefusalDeltaBlock, ImageURLDeltaBlock],
