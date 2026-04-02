__all__ = ["VideoExtendParams", "Video", "VideoVideoReferenceInputParam"]
class VideoExtendParams(TypedDict, total=False):
    """Updated text prompt that directs the extension generation."""
    seconds: Required[VideoSeconds]
    Length of the newly generated extension segment in seconds (allowed values: 4,
    """Reference to the completed video to extend."""
