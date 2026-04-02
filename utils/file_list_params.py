__all__ = ["FileListParams"]
class FileListParams(TypedDict, total=False):
    Limit can range between 1 and 10,000, and the default is 10,000.
    """Only return files with the given purpose."""
