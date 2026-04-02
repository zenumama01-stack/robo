from .dpo_method import DpoMethod
from .supervised_method import SupervisedMethod
from .reinforcement_method import ReinforcementMethod
from .fine_tuning_job_wandb_integration_object import FineTuningJobWandbIntegrationObject
__all__ = ["FineTuningJob", "Error", "Hyperparameters", "Method"]
    For fine-tuning jobs that have `failed`, this will contain more information on the cause of the failure.
    """A machine-readable error code."""
    """The parameter that was invalid, usually `training_file` or `validation_file`.
    This field will be null if the failure was not parameter-specific.
class Hyperparameters(BaseModel):
    """The hyperparameters used for the fine-tuning job.
    This value will only be returned when running `supervised` jobs.
class Method(BaseModel):
    """The method used for fine-tuning."""
    type: Literal["supervised", "dpo", "reinforcement"]
    """The type of method. Is either `supervised`, `dpo`, or `reinforcement`."""
    dpo: Optional[DpoMethod] = None
    reinforcement: Optional[ReinforcementMethod] = None
    """Configuration for the reinforcement fine-tuning method."""
    supervised: Optional[SupervisedMethod] = None
    """Configuration for the supervised fine-tuning method."""
class FineTuningJob(BaseModel):
    The `fine_tuning.job` object represents a fine-tuning job that has been created through the API.
    """The object identifier, which can be referenced in the API endpoints."""
    """The Unix timestamp (in seconds) for when the fine-tuning job was created."""
    For fine-tuning jobs that have `failed`, this will contain more information on
    the cause of the failure.
    fine_tuned_model: Optional[str] = None
    """The name of the fine-tuned model that is being created.
    The value will be null if the fine-tuning job is still running.
    finished_at: Optional[int] = None
    """The Unix timestamp (in seconds) for when the fine-tuning job was finished.
    hyperparameters: Hyperparameters
    """The base model that is being fine-tuned."""
    object: Literal["fine_tuning.job"]
    """The object type, which is always "fine_tuning.job"."""
    organization_id: str
    """The organization that owns the fine-tuning job."""
    result_files: List[str]
    """The compiled results file ID(s) for the fine-tuning job.
    You can retrieve the results with the
    [Files API](https://platform.openai.com/docs/api-reference/files/retrieve-contents).
    """The seed used for the fine-tuning job."""
    status: Literal["validating_files", "queued", "running", "succeeded", "failed", "cancelled"]
    The current status of the fine-tuning job, which can be either
    `validating_files`, `queued`, `running`, `succeeded`, `failed`, or `cancelled`.
    trained_tokens: Optional[int] = None
    """The total number of billable tokens processed by this fine-tuning job.
    """The file ID used for training.
    You can retrieve the training data with the
    validation_file: Optional[str] = None
    """The file ID used for validation.
    You can retrieve the validation results with the
    estimated_finish: Optional[int] = None
    The Unix timestamp (in seconds) for when the fine-tuning job is estimated to
    finish. The value will be null if the fine-tuning job is not running.
    integrations: Optional[List[FineTuningJobWandbIntegrationObject]] = None
    """A list of integrations to enable for this fine-tuning job."""
    method: Optional[Method] = None
