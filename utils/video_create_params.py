from typing_extensions import Required, TypeAlias, TypedDict
from .video_model_param import VideoModelParam
from .image_input_reference_param import ImageInputReferenceParam
__all__ = ["VideoCreateParams", "InputReference"]
class VideoCreateParams(TypedDict, total=False):
    """Text prompt that describes the video to generate."""
    input_reference: InputReference
    """Optional reference asset upload or reference object that guides generation."""
    model: VideoModelParam
    """The video generation model to use (allowed values: sora-2, sora-2-pro).
    Defaults to `sora-2`.
    seconds: VideoSeconds
    """Clip duration in seconds (allowed values: 4, 8, 12). Defaults to 4 seconds."""
    Output resolution formatted as width x height (allowed values: 720x1280,
InputReference: TypeAlias = Union[FileTypes, ImageInputReferenceParam]
__all__ = ["VideoCreateParams"]
    input_reference: FileTypes
    """Optional image reference that guides generation."""
