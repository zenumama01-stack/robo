__all__ = ["CodeInterpreterToolParam"]
class CodeInterpreterToolParam(TypedDict, total=False):
    type: Required[Literal["code_interpreter"]]
