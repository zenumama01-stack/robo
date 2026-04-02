__all__ = ["RunStepDeltaMessageDelta", "MessageCreation"]
    message_id: Optional[str] = None
class RunStepDeltaMessageDelta(BaseModel):
    message_creation: Optional[MessageCreation] = None
