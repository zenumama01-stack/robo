__all__ = ["EvalRunFailedWebhookEvent", "Data"]
class EvalRunFailedWebhookEvent(BaseModel):
    """Sent when an eval run has failed."""
    """The Unix timestamp (in seconds) of when the eval run failed."""
    type: Literal["eval.run.failed"]
    """The type of the event. Always `eval.run.failed`."""
