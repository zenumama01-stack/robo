from .function_tool import FunctionTool
from .file_search_tool import FileSearchTool
from .code_interpreter_tool import CodeInterpreterTool
__all__ = ["AssistantTool"]
AssistantTool: TypeAlias = Annotated[
    Union[CodeInterpreterTool, FileSearchTool, FunctionTool], PropertyInfo(discriminator="type")
