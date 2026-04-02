from .assistant_tool_choice import AssistantToolChoice
__all__ = ["AssistantToolChoiceOption"]
AssistantToolChoiceOption: TypeAlias = Union[Literal["none", "auto", "required"], AssistantToolChoice]
