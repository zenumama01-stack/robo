__all__ = ["VectorStoreFileBatch", "FileCounts"]
    """The number of files that where cancelled."""
    """The number of files that have been processed."""
class VectorStoreFileBatch(BaseModel):
    """A batch of files attached to a vector store."""
    The Unix timestamp (in seconds) for when the vector store files batch was
    object: Literal["vector_store.files_batch"]
    """The object type, which is always `vector_store.file_batch`."""
    The status of the vector store files batch, which can be either `in_progress`,
    `completed`, `cancelled` or `failed`.
