__all__ = ["ImageInputReferenceParam"]
class ImageInputReferenceParam(TypedDict, total=False):
    file_id: str
    image_url: str
    """A fully qualified URL or base64-encoded data URL."""
