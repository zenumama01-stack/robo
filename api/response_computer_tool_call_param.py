from .computer_action_list_param import ComputerActionListParam
    "ResponseComputerToolCallParam",
class PendingSafetyCheck(TypedDict, total=False):
    code: Optional[str]
    message: Optional[str]
class ActionClick(TypedDict, total=False):
class ActionDoubleClick(TypedDict, total=False):
class ActionDragPath(TypedDict, total=False):
class ActionDrag(TypedDict, total=False):
    path: Required[Iterable[ActionDragPath]]
class ActionKeypress(TypedDict, total=False):
class ActionMove(TypedDict, total=False):
class ActionScreenshot(TypedDict, total=False):
class ActionScroll(TypedDict, total=False):
class ActionType(TypedDict, total=False):
class ActionWait(TypedDict, total=False):
Action: TypeAlias = Union[
class ResponseComputerToolCallParam(TypedDict, total=False):
    pending_safety_checks: Required[Iterable[PendingSafetyCheck]]
    status: Required[Literal["in_progress", "completed", "incomplete"]]
    type: Required[Literal["computer_call"]]
    action: Action
    actions: ComputerActionListParam
