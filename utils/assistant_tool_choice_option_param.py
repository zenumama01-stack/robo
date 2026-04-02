from .assistant_tool_choice_param import AssistantToolChoiceParam
__all__ = ["AssistantToolChoiceOptionParam"]
AssistantToolChoiceOptionParam: TypeAlias = Union[Literal["none", "auto", "required"], AssistantToolChoiceParam]
