__all__ = ["RealtimeConversationItemSystemMessage", "Content"]
    type: Optional[Literal["input_text"]] = None
    """The content type. Always `input_text` for system messages."""
class RealtimeConversationItemSystemMessage(BaseModel):
    A system message in a Realtime conversation can be used to provide additional context or instructions to the model. This is similar but distinct from the instruction prompt provided at the start of a conversation, as system messages can be added at any point in the conversation. For major changes to the conversation's behavior, use instructions, but for smaller updates (e.g. "the user is now asking about a different topic"), use system messages.
    role: Literal["system"]
    """The role of the message sender. Always `system`."""
