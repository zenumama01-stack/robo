from .function_tool_call_delta import FunctionToolCallDelta
from .file_search_tool_call_delta import FileSearchToolCallDelta
from .code_interpreter_tool_call_delta import CodeInterpreterToolCallDelta
__all__ = ["ToolCallDelta"]
ToolCallDelta: TypeAlias = Annotated[
    Union[CodeInterpreterToolCallDelta, FileSearchToolCallDelta, FunctionToolCallDelta],
