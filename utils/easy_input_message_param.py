from .response_input_message_content_list_param import ResponseInputMessageContentListParam
__all__ = ["EasyInputMessageParam"]
class EasyInputMessageParam(TypedDict, total=False):
    content: Required[Union[str, ResponseInputMessageContentListParam]]
    phase: Optional[Literal["commentary", "final_answer"]]
