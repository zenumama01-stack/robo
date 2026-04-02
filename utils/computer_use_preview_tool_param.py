__all__ = ["ComputerUsePreviewToolParam"]
class ComputerUsePreviewToolParam(TypedDict, total=False):
    display_height: Required[int]
    display_width: Required[int]
    environment: Required[Literal["windows", "mac", "linux", "ubuntu", "browser"]]
    type: Required[Literal["computer_use_preview"]]
