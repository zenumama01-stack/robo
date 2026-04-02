from .assistant_tool_choice_function_param import AssistantToolChoiceFunctionParam
__all__ = ["AssistantToolChoiceParam"]
class AssistantToolChoiceParam(TypedDict, total=False):
    type: Required[Literal["function", "code_interpreter", "file_search"]]
    function: AssistantToolChoiceFunctionParam
