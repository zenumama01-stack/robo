from .fine_tuning_job_wandb_integration import FineTuningJobWandbIntegration
__all__ = ["FineTuningJobWandbIntegrationObject"]
class FineTuningJobWandbIntegrationObject(BaseModel):
    type: Literal["wandb"]
    """The type of the integration being enabled for the fine-tuning job"""
    wandb: FineTuningJobWandbIntegration
    This payload specifies the project that metrics will be sent to. Optionally, you
    can set an explicit display name for your run, add tags to your run, and set a
    default entity (team, username, etc) to be associated with your run.
