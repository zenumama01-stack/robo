__all__ = ["ResponseFunctionShellCallOutputContentParam", "Outcome", "OutcomeTimeout", "OutcomeExit"]
class OutcomeTimeout(TypedDict, total=False):
    type: Required[Literal["timeout"]]
class OutcomeExit(TypedDict, total=False):
    exit_code: Required[int]
    type: Required[Literal["exit"]]
Outcome: TypeAlias = Union[OutcomeTimeout, OutcomeExit]
class ResponseFunctionShellCallOutputContentParam(TypedDict, total=False):
    outcome: Required[Outcome]
    stderr: Required[str]
    stdout: Required[str]
