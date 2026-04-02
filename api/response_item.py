from .response_input_message_item import ResponseInputMessageItem
from .response_custom_tool_call_item import ResponseCustomToolCallItem
from .response_function_tool_call_item import ResponseFunctionToolCallItem
    "ResponseItem",
ResponseItem: TypeAlias = Annotated[
        ResponseInputMessageItem,
        ResponseCustomToolCallItem,
