from ....._utils import PropertyInfo
    "CodeInterpreterToolCall",
    "CodeInterpreter",
    "CodeInterpreterOutput",
    "CodeInterpreterOutputLogs",
    "CodeInterpreterOutputImage",
    "CodeInterpreterOutputImageImage",
class CodeInterpreterOutputLogs(BaseModel):
    logs: str
class CodeInterpreterOutputImageImage(BaseModel):
    image: CodeInterpreterOutputImageImage
CodeInterpreterOutput: TypeAlias = Annotated[
    Union[CodeInterpreterOutputLogs, CodeInterpreterOutputImage], PropertyInfo(discriminator="type")
class CodeInterpreter(BaseModel):
    """The Code Interpreter tool call definition."""
    input: str
    """The input to the Code Interpreter tool call."""
    outputs: List[CodeInterpreterOutput]
    """The outputs from the Code Interpreter tool call.
    Code Interpreter can output one or more items, including text (`logs`) or images
    (`image`). Each of these are represented by a different object type.
class CodeInterpreterToolCall(BaseModel):
    """Details of the Code Interpreter tool call the run step was involved in."""
    """The ID of the tool call."""
    code_interpreter: CodeInterpreter
    """The type of tool call.
    This is always going to be `code_interpreter` for this type of tool call.
