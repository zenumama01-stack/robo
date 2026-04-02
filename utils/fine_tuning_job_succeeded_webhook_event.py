__all__ = ["FineTuningJobSucceededWebhookEvent", "Data"]
class FineTuningJobSucceededWebhookEvent(BaseModel):
    """Sent when a fine-tuning job has succeeded."""
    """The Unix timestamp (in seconds) of when the fine-tuning job succeeded."""
    type: Literal["fine_tuning.job.succeeded"]
    """The type of the event. Always `fine_tuning.job.succeeded`."""
