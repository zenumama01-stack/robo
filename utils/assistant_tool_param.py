from .function_tool_param import FunctionToolParam
from .file_search_tool_param import FileSearchToolParam
from .code_interpreter_tool_param import CodeInterpreterToolParam
__all__ = ["AssistantToolParam"]
AssistantToolParam: TypeAlias = Union[CodeInterpreterToolParam, FileSearchToolParam, FunctionToolParam]
