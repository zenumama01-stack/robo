from .._types import FileTypes
from .file_purpose import FilePurpose
__all__ = ["FileCreateParams", "ExpiresAfter"]
class FileCreateParams(TypedDict, total=False):
    file: Required[FileTypes]
    """The File object (not file name) to be uploaded."""
    purpose: Required[FilePurpose]
    """The intended purpose of the uploaded file. One of:
    """The expiration policy for a file.
    By default, files with `purpose=batch` expire after 30 days and all other files
    are persisted until they are manually deleted.
    By default, files with `purpose=batch` expire after 30 days and all other files are persisted until they are manually deleted.
    Supported anchors: `created_at`.
__all__ = ["FileCreateParams"]
    file: FileTypes
    """Name of the file to create."""
