__all__ = ["ComputerToolParam"]
class ComputerToolParam(TypedDict, total=False):
    type: Required[Literal["computer"]]
