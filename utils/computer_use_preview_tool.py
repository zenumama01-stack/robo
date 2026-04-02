__all__ = ["ComputerUsePreviewTool"]
class ComputerUsePreviewTool(BaseModel):
    display_height: int
    """The height of the computer display."""
    display_width: int
    """The width of the computer display."""
    environment: Literal["windows", "mac", "linux", "ubuntu", "browser"]
    """The type of computer environment to control."""
    type: Literal["computer_use_preview"]
    """The type of the computer use tool. Always `computer_use_preview`."""
