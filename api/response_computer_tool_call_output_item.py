from .response_computer_tool_call_output_screenshot import ResponseComputerToolCallOutputScreenshot
__all__ = ["ResponseComputerToolCallOutputItem", "AcknowledgedSafetyCheck"]
class AcknowledgedSafetyCheck(BaseModel):
class ResponseComputerToolCallOutputItem(BaseModel):
    """The unique ID of the computer call tool output."""
    """The ID of the computer tool call that produced the output."""
    output: ResponseComputerToolCallOutputScreenshot
    """A computer screenshot image used with the computer use tool."""
    status: Literal["completed", "incomplete", "failed", "in_progress"]
    """The status of the message input.
    One of `in_progress`, `completed`, or `incomplete`. Populated when input items
    are returned via API.
    type: Literal["computer_call_output"]
    """The type of the computer tool call output. Always `computer_call_output`."""
    acknowledged_safety_checks: Optional[List[AcknowledgedSafetyCheck]] = None
    The safety checks reported by the API that have been acknowledged by the
    developer.
