__all__ = ["FineTuningJobFailedWebhookEvent", "Data"]
class FineTuningJobFailedWebhookEvent(BaseModel):
    """Sent when a fine-tuning job has failed."""
    """The Unix timestamp (in seconds) of when the fine-tuning job failed."""
    type: Literal["fine_tuning.job.failed"]
    """The type of the event. Always `fine_tuning.job.failed`."""
