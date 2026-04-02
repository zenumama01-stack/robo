__all__ = ["EvalRunCanceledWebhookEvent", "Data"]
    """The unique ID of the eval run."""
class EvalRunCanceledWebhookEvent(BaseModel):
    """Sent when an eval run has been canceled."""
    """The Unix timestamp (in seconds) of when the eval run was canceled."""
    type: Literal["eval.run.canceled"]
    """The type of the event. Always `eval.run.canceled`."""
