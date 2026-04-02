from .image_edit_completed_event import ImageEditCompletedEvent
from .image_edit_partial_image_event import ImageEditPartialImageEvent
__all__ = ["ImageEditStreamEvent"]
ImageEditStreamEvent: TypeAlias = Annotated[
    Union[ImageEditPartialImageEvent, ImageEditCompletedEvent], PropertyInfo(discriminator="type")
