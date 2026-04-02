from ....shared.metadata import Metadata
from .tool_calls_step_details import ToolCallsStepDetails
from .message_creation_step_details import MessageCreationStepDetails
__all__ = ["RunStep", "LastError", "StepDetails", "Usage"]
    """The last error associated with this run step.
    Will be `null` if there are no errors.
    code: Literal["server_error", "rate_limit_exceeded"]
    """One of `server_error` or `rate_limit_exceeded`."""
StepDetails: TypeAlias = Annotated[
    Union[MessageCreationStepDetails, ToolCallsStepDetails], PropertyInfo(discriminator="type")
    """Usage statistics related to the run step.
    This value will be `null` while the run step's status is `in_progress`.
    """Number of completion tokens used over the course of the run step."""
    """Number of prompt tokens used over the course of the run step."""
class RunStep(BaseModel):
    """The identifier of the run step, which can be referenced in API endpoints."""
    [assistant](https://platform.openai.com/docs/api-reference/assistants)
    associated with the run step.
    """The Unix timestamp (in seconds) for when the run step was cancelled."""
    """The Unix timestamp (in seconds) for when the run step completed."""
    """The Unix timestamp (in seconds) for when the run step was created."""
    """The Unix timestamp (in seconds) for when the run step expired.
    A step is considered expired if the parent run is expired.
    """The Unix timestamp (in seconds) for when the run step failed."""
    object: Literal["thread.run.step"]
    """The object type, which is always `thread.run.step`."""
    The ID of the [run](https://platform.openai.com/docs/api-reference/runs) that
    this run step is a part of.
    status: Literal["in_progress", "cancelled", "failed", "completed", "expired"]
    The status of the run step, which can be either `in_progress`, `cancelled`,
    `failed`, `completed`, or `expired`.
    step_details: StepDetails
    """The details of the run step."""
    that was run.
    type: Literal["message_creation", "tool_calls"]
    """The type of run step, which can be either `message_creation` or `tool_calls`."""
