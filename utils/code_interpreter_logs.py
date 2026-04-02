from ....._models import BaseModel
__all__ = ["CodeInterpreterLogs"]
class CodeInterpreterLogs(BaseModel):
    """Text output from the Code Interpreter tool call as part of a run step."""
    """The index of the output in the outputs array."""
    """Always `logs`."""
    logs: Optional[str] = None
    """The text output from the Code Interpreter tool call."""
