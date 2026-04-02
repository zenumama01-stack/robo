__all__ = ["ChatSessionFileUpload"]
class ChatSessionFileUpload(BaseModel):
    """Upload permissions and limits applied to the session."""
    """Indicates if uploads are enabled for the session."""
    max_file_size: Optional[int] = None
    """Maximum upload size in megabytes."""
    max_files: Optional[int] = None
    """Maximum number of uploads allowed during the session."""
