__all__ = ["ComputerTool"]
class ComputerTool(BaseModel):
    """A tool that controls a virtual computer.
    Learn more about the [computer tool](https://platform.openai.com/docs/guides/tools-computer-use).
    type: Literal["computer"]
    """The type of the computer tool. Always `computer`."""
