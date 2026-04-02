from ..file_chunking_strategy import FileChunkingStrategy
__all__ = ["VectorStoreFile", "LastError"]
    """The last error associated with this vector store file.
    code: Literal["server_error", "unsupported_file", "invalid_file"]
    """One of `server_error`, `unsupported_file`, or `invalid_file`."""
class VectorStoreFile(BaseModel):
    """A list of files attached to a vector store."""
    """The Unix timestamp (in seconds) for when the vector store file was created."""
    object: Literal["vector_store.file"]
    """The object type, which is always `vector_store.file`."""
    status: Literal["in_progress", "completed", "cancelled", "failed"]
    The status of the vector store file, which can be either `in_progress`,
    `completed`, `cancelled`, or `failed`. The status `completed` indicates that the
    vector store file is ready for use.
    """The total vector store usage in bytes.
    Note that this may be different from the original file size.
    vector_store_id: str
    that the [File](https://platform.openai.com/docs/api-reference/files) is
    attached to.
    chunking_strategy: Optional[FileChunkingStrategy] = None
    """The strategy used to chunk the file."""
