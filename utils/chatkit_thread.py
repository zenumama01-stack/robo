__all__ = ["ChatKitThread", "Status", "StatusActive", "StatusLocked", "StatusClosed"]
class StatusActive(BaseModel):
    """Indicates that a thread is active."""
    type: Literal["active"]
    """Status discriminator that is always `active`."""
class StatusLocked(BaseModel):
    """Indicates that a thread is locked and cannot accept new input."""
    reason: Optional[str] = None
    """Reason that the thread was locked. Defaults to null when no reason is recorded."""
    type: Literal["locked"]
    """Status discriminator that is always `locked`."""
class StatusClosed(BaseModel):
    """Indicates that a thread has been closed."""
    """Reason that the thread was closed. Defaults to null when no reason is recorded."""
    type: Literal["closed"]
    """Status discriminator that is always `closed`."""
Status: TypeAlias = Annotated[Union[StatusActive, StatusLocked, StatusClosed], PropertyInfo(discriminator="type")]
class ChatKitThread(BaseModel):
    """Represents a ChatKit thread and its current status."""
    """Identifier of the thread."""
    """Unix timestamp (in seconds) for when the thread was created."""
    object: Literal["chatkit.thread"]
    """Type discriminator that is always `chatkit.thread`."""
    status: Status
    """Current status for the thread. Defaults to `active` for newly created threads."""
    title: Optional[str] = None
    """Optional human-readable title for the thread.
    Defaults to null when no title has been generated.
    """Free-form string that identifies your end user who owns the thread."""
