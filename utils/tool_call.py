from .function_tool_call import FunctionToolCall
from .file_search_tool_call import FileSearchToolCall
from .code_interpreter_tool_call import CodeInterpreterToolCall
__all__ = ["ToolCall"]
ToolCall: TypeAlias = Annotated[
    Union[CodeInterpreterToolCall, FileSearchToolCall, FunctionToolCall], PropertyInfo(discriminator="type")
