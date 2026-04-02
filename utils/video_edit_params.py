__all__ = ["VideoEditParams", "Video", "VideoVideoReferenceInputParam"]
class VideoEditParams(TypedDict, total=False):
    """Text prompt that describes how to edit the source video."""
    video: Required[Video]
    """Reference to the completed video to edit."""
class VideoVideoReferenceInputParam(TypedDict, total=False):
    """Reference to the completed video."""
    id: Required[str]
    """The identifier of the completed video."""
Video: TypeAlias = Union[FileTypes, VideoVideoReferenceInputParam]
