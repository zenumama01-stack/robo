__all__ = ["ResponsesClientEventParam", "ContextManagement", "Conversation", "StreamOptions", "ToolChoice"]
class ResponsesClientEventParam(TypedDict, total=False):
    stream: Optional[bool]
