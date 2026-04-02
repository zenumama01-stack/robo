from .computer_action_list import ComputerActionList
    "ResponseComputerToolCall",
    "PendingSafetyCheck",
    "Action",
    "ActionClick",
    "ActionDoubleClick",
    "ActionDrag",
    "ActionDragPath",
    "ActionKeypress",
    "ActionMove",
    "ActionScreenshot",
    "ActionScroll",
    "ActionType",
    "ActionWait",
class PendingSafetyCheck(BaseModel):
    """A pending safety check for the computer call."""
    """The ID of the pending safety check."""
    """The type of the pending safety check."""
    """Details about the pending safety check."""
class ActionClick(BaseModel):
class ActionDoubleClick(BaseModel):
class ActionDragPath(BaseModel):
class ActionDrag(BaseModel):
    path: List[ActionDragPath]
class ActionKeypress(BaseModel):
class ActionMove(BaseModel):
class ActionScreenshot(BaseModel):
class ActionScroll(BaseModel):
class ActionType(BaseModel):
class ActionWait(BaseModel):
Action: TypeAlias = Annotated[
        ActionClick,
        ActionDoubleClick,
        ActionDrag,
        ActionKeypress,
        ActionMove,
        ActionScreenshot,
        ActionScroll,
        ActionType,
        ActionWait,
class ResponseComputerToolCall(BaseModel):
    """A tool call to a computer use tool.
    [computer use guide](https://platform.openai.com/docs/guides/tools-computer-use) for more information.
    """The unique ID of the computer call."""
    """An identifier used when responding to the tool call with output."""
    pending_safety_checks: List[PendingSafetyCheck]
    """The pending safety checks for the computer call."""
    """The status of the item.
    type: Literal["computer_call"]
    """The type of the computer call. Always `computer_call`."""
    action: Optional[Action] = None
    actions: Optional[ComputerActionList] = None
    """Flattened batched actions for `computer_use`.
    Each action includes an `type` discriminator and action-specific fields.
