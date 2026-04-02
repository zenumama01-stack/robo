__all__ = ["FileObject"]
class FileObject(BaseModel):
    """The `File` object represents a document that has been uploaded to OpenAI."""
    """The file identifier, which can be referenced in the API endpoints."""
    bytes: int
    """The size of the file, in bytes."""
    """The Unix timestamp (in seconds) for when the file was created."""
    filename: str
    """The name of the file."""
    """The object type, which is always `file`."""
    purpose: Literal[
        "assistants",
        "assistants_output",
        "batch",
        "batch_output",
        "fine-tune",
        "fine-tune-results",
        "vision",
        "user_data",
    """The intended purpose of the file.
    Supported values are `assistants`, `assistants_output`, `batch`, `batch_output`,
    `fine-tune`, `fine-tune-results`, `vision`, and `user_data`.
    status: Literal["uploaded", "processed", "error"]
    """Deprecated.
    The current status of the file, which can be either `uploaded`, `processed`, or
    `error`.
    """The Unix timestamp (in seconds) for when the file will expire."""
    status_details: Optional[str] = None
    For details on why a fine-tuning training file failed validation, see the
    `error` field on `fine_tuning.job`.
