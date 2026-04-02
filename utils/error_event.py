__all__ = ["ErrorEvent", "Error"]
    type: str
    """The type of error (e.g., "invalid_request_error", "server_error")."""
    """The event_id of the client event that caused the error, if applicable."""
    """Details of the error."""
    type: Literal["error"]
    """The event type, must be `error`."""
