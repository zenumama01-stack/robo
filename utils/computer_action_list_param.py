    "ComputerActionListParam",
    "ComputerActionParam",
class Click(TypedDict, total=False):
    button: Required[Literal["left", "right", "wheel", "back", "forward"]]
    type: Required[Literal["click"]]
    x: Required[int]
    y: Required[int]
    keys: Optional[SequenceNotStr[str]]
class DoubleClick(TypedDict, total=False):
    keys: Required[Optional[SequenceNotStr[str]]]
    type: Required[Literal["double_click"]]
class DragPath(TypedDict, total=False):
class Drag(TypedDict, total=False):
    path: Required[Iterable[DragPath]]
    type: Required[Literal["drag"]]
class Keypress(TypedDict, total=False):
    keys: Required[SequenceNotStr[str]]
    type: Required[Literal["keypress"]]
class Move(TypedDict, total=False):
    type: Required[Literal["move"]]
class Screenshot(TypedDict, total=False):
    type: Required[Literal["screenshot"]]
class Scroll(TypedDict, total=False):
    scroll_x: Required[int]
    scroll_y: Required[int]
    type: Required[Literal["scroll"]]
class Type(TypedDict, total=False):
    type: Required[Literal["type"]]
class Wait(TypedDict, total=False):
    type: Required[Literal["wait"]]
ComputerActionParam: TypeAlias = Union[Click, DoubleClick, Drag, Keypress, Move, Screenshot, Scroll, Type, Wait]
ComputerActionListParam: TypeAlias = List[ComputerActionParam]
