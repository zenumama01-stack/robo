__all__ = ["EvalRunSucceededWebhookEvent", "Data"]
class EvalRunSucceededWebhookEvent(BaseModel):
    """Sent when an eval run has succeeded."""
    """The Unix timestamp (in seconds) of when the eval run succeeded."""
    type: Literal["eval.run.succeeded"]
    """The type of the event. Always `eval.run.succeeded`."""
