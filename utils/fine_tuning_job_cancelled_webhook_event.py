__all__ = ["FineTuningJobCancelledWebhookEvent", "Data"]
    """The unique ID of the fine-tuning job."""
class FineTuningJobCancelledWebhookEvent(BaseModel):
    """Sent when a fine-tuning job has been cancelled."""
    """The Unix timestamp (in seconds) of when the fine-tuning job was cancelled."""
    type: Literal["fine_tuning.job.cancelled"]
    """The type of the event. Always `fine_tuning.job.cancelled`."""
