__all__ = ["VideoDownloadContentParams"]
class VideoDownloadContentParams(TypedDict, total=False):
    variant: Literal["video", "thumbnail", "spritesheet"]
    """Which downloadable asset to return. Defaults to the MP4 video."""
