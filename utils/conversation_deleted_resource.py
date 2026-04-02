__all__ = ["ConversationDeletedResource"]
class ConversationDeletedResource(BaseModel):
    object: Literal["conversation.deleted"]
