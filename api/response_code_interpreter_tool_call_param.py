__all__ = ["ResponseCodeInterpreterToolCallParam", "Output", "OutputLogs", "OutputImage"]
class OutputLogs(TypedDict, total=False):
    logs: Required[str]
class OutputImage(TypedDict, total=False):
    type: Required[Literal["image"]]
Output: TypeAlias = Union[OutputLogs, OutputImage]
class ResponseCodeInterpreterToolCallParam(TypedDict, total=False):
    code: Required[Optional[str]]
    outputs: Required[Optional[Iterable[Output]]]
    status: Required[Literal["in_progress", "completed", "incomplete", "interpreting", "failed"]]
    type: Required[Literal["code_interpreter_call"]]
