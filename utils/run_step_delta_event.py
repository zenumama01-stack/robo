from .run_step_delta import RunStepDelta
__all__ = ["RunStepDeltaEvent"]
class RunStepDeltaEvent(BaseModel):
    delta: RunStepDelta
    object: Literal["thread.run.step.delta"]
    """The object type, which is always `thread.run.step.delta`."""
