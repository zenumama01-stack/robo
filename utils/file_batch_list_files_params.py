__all__ = ["FileBatchListFilesParams"]
class FileBatchListFilesParams(TypedDict, total=False):
    vector_store_id: Required[str]
    filter: Literal["in_progress", "completed", "failed", "cancelled"]
    """Filter by file status.
    One of `in_progress`, `completed`, `failed`, `cancelled`.
