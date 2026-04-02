from .tool_call_delta_object import ToolCallDeltaObject
from .run_step_delta_message_delta import RunStepDeltaMessageDelta
__all__ = ["RunStepDelta", "StepDetails"]
    Union[RunStepDeltaMessageDelta, ToolCallDeltaObject], PropertyInfo(discriminator="type")
class RunStepDelta(BaseModel):
    """The delta containing the fields that have changed on the run step."""
    step_details: Optional[StepDetails] = None
