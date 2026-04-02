__all__ = ["FineTuningJobEvent"]
class FineTuningJobEvent(BaseModel):
    """Fine-tuning job event object"""
    """The object identifier."""
    level: Literal["info", "warn", "error"]
    """The log level of the event."""
    """The message of the event."""
    object: Literal["fine_tuning.job.event"]
    """The object type, which is always "fine_tuning.job.event"."""
    data: Optional[builtins.object] = None
    """The data associated with the event."""
    type: Optional[Literal["message", "metrics"]] = None
    """The type of event."""
