__all__ = ["VideoCreateCharacterParams"]
class VideoCreateCharacterParams(TypedDict, total=False):
    """Display name for this API character."""
    video: Required[FileTypes]
    """Video file used to create a character."""
