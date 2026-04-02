from .response_input_message_content_list import ResponseInputMessageContentList
__all__ = ["EasyInputMessage"]
class EasyInputMessage(BaseModel):
    content: Union[str, ResponseInputMessageContentList]
    Text, image, or audio input to the model, used to generate a response. Can also
    contain previous assistant responses.
    phase: Optional[Literal["commentary", "final_answer"]] = None
    Labels an `assistant` message as intermediate commentary (`commentary`) or the
    final answer (`final_answer`). For models like `gpt-5.3-codex` and beyond, when
    sending follow-up requests, preserve and resend phase on all assistant messages
    — dropping it can degrade performance. Not used for user messages.
