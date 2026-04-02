__all__ = ["PermissionCreateResponse"]
class PermissionCreateResponse(BaseModel):
    The `checkpoint.permission` object represents a permission for a fine-tuned model checkpoint.
    """The permission identifier, which can be referenced in the API endpoints."""
    """The Unix timestamp (in seconds) for when the permission was created."""
    object: Literal["checkpoint.permission"]
    """The object type, which is always "checkpoint.permission"."""
    project_id: str
    """The project identifier that the permission is for."""
