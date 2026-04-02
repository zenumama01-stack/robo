__all__ = ["ConversationItemWithReference", "Content"]
    """The transcript of the audio, used for `input_audio` content type."""
    type: Optional[Literal["input_text", "input_audio", "item_reference", "text"]] = None
    """The content type (`input_text`, `input_audio`, `item_reference`, `text`)."""
class ConversationItemWithReference(BaseModel):
    For an item of type (`message` | `function_call` | `function_call_output`) this
    field allows the client to assign the unique ID of the item. It is not required
    because the server will generate one if not provided.
    For an item of type `item_reference`, this field is required and is a reference
    to any item that has previously existed in the conversation.
    content: Optional[List[Content]] = None
    type: Optional[Literal["message", "function_call", "function_call_output", "item_reference"]] = None
    The type of the item (`message`, `function_call`, `function_call_output`,
    `item_reference`).
