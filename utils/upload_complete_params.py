__all__ = ["UploadCompleteParams"]
class UploadCompleteParams(TypedDict, total=False):
    part_ids: Required[SequenceNotStr[str]]
    """The ordered list of Part IDs."""
    md5: str
    The optional md5 checksum for the file contents to verify if the bytes uploaded
