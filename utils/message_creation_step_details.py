__all__ = ["MessageCreationStepDetails", "MessageCreation"]
class MessageCreation(BaseModel):
    message_id: str
    """The ID of the message that was created by this run step."""
class MessageCreationStepDetails(BaseModel):
    """Details of the message creation by the run step."""
    message_creation: MessageCreation
    type: Literal["message_creation"]
    """Always `message_creation`."""
