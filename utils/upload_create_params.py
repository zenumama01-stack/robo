__all__ = ["UploadCreateParams", "ExpiresAfter"]
class UploadCreateParams(TypedDict, total=False):
    bytes: Required[int]
    """The number of bytes in the file you are uploading."""
    filename: Required[str]
    """The name of the file to upload."""
    mime_type: Required[str]
    """The MIME type of the file.
    """The intended purpose of the uploaded file.
