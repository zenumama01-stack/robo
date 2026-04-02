__all__ = ["VectorStore", "FileCounts", "ExpiresAfter"]
class FileCounts(BaseModel):
    cancelled: int
    """The number of files that were cancelled."""
    """The number of files that have been successfully processed."""
    """The number of files that have failed to process."""
    in_progress: int
    """The number of files that are currently being processed."""
    """The total number of files."""
    """The expiration policy for a vector store."""
    anchor: Literal["last_active_at"]
    Supported anchors: `last_active_at`.
    days: int
    """The number of days after the anchor time that the vector store will expire."""
class VectorStore(BaseModel):
    A vector store is a collection of processed files can be used by the `file_search` tool.
    """The identifier, which can be referenced in API endpoints."""
    """The Unix timestamp (in seconds) for when the vector store was created."""
    file_counts: FileCounts
    """The Unix timestamp (in seconds) for when the vector store was last active."""
    """The name of the vector store."""
    object: Literal["vector_store"]
    """The object type, which is always `vector_store`."""
    status: Literal["expired", "in_progress", "completed"]
    The status of the vector store, which can be either `expired`, `in_progress`, or
    `completed`. A status of `completed` indicates that the vector store is ready
    for use.
    usage_bytes: int
    """The total number of bytes used by the files in the vector store."""
    """The Unix timestamp (in seconds) for when the vector store will expire."""
