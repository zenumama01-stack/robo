__all__ = ["CodeInterpreterTool"]
class CodeInterpreterTool(BaseModel):
    type: Literal["code_interpreter"]
    """The type of tool being defined: `code_interpreter`"""
