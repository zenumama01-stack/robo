from .code_interpreter_logs import CodeInterpreterLogs
from .code_interpreter_output_image import CodeInterpreterOutputImage
__all__ = ["CodeInterpreterToolCallDelta", "CodeInterpreter", "CodeInterpreterOutput"]
    Union[CodeInterpreterLogs, CodeInterpreterOutputImage], PropertyInfo(discriminator="type")
    input: Optional[str] = None
    outputs: Optional[List[CodeInterpreterOutput]] = None
class CodeInterpreterToolCallDelta(BaseModel):
    """The index of the tool call in the tool calls array."""
    code_interpreter: Optional[CodeInterpreter] = None
