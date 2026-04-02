    "ComputerAction",
    "Click",
    "DoubleClick",
    "Drag",
    "DragPath",
    "Keypress",
    "Move",
    "Screenshot",
    "Scroll",
    "Wait",
class Click(BaseModel):
    """A click action."""
    button: Literal["left", "right", "wheel", "back", "forward"]
    """Indicates which mouse button was pressed during the click.
    One of `left`, `right`, `wheel`, `back`, or `forward`.
    type: Literal["click"]
    """Specifies the event type. For a click action, this property is always `click`."""
    x: int
    """The x-coordinate where the click occurred."""
    y: int
    """The y-coordinate where the click occurred."""
    keys: Optional[List[str]] = None
    """The keys being held while clicking."""
class DoubleClick(BaseModel):
    """A double click action."""
    """The keys being held while double-clicking."""
    type: Literal["double_click"]
    For a double click action, this property is always set to `double_click`.
    """The x-coordinate where the double click occurred."""
    """The y-coordinate where the double click occurred."""
class DragPath(BaseModel):
    """An x/y coordinate pair, e.g. `{ x: 100, y: 200 }`."""
    """The x-coordinate."""
    """The y-coordinate."""
class Drag(BaseModel):
    """A drag action."""
    path: List[DragPath]
    """An array of coordinates representing the path of the drag action.
    Coordinates will appear as an array of objects, eg
      { x: 100, y: 200 },
      { x: 200, y: 300 }
    type: Literal["drag"]
    For a drag action, this property is always set to `drag`.
    """The keys being held while dragging the mouse."""
class Keypress(BaseModel):
    """A collection of keypresses the model would like to perform."""
    keys: List[str]
    """The combination of keys the model is requesting to be pressed.
    This is an array of strings, each representing a key.
    type: Literal["keypress"]
    For a keypress action, this property is always set to `keypress`.
class Move(BaseModel):
    """A mouse move action."""
    type: Literal["move"]
    For a move action, this property is always set to `move`.
    """The x-coordinate to move to."""
    """The y-coordinate to move to."""
    """The keys being held while moving the mouse."""
class Screenshot(BaseModel):
    """A screenshot action."""
    type: Literal["screenshot"]
    For a screenshot action, this property is always set to `screenshot`.
class Scroll(BaseModel):
    """A scroll action."""
    scroll_x: int
    """The horizontal scroll distance."""
    scroll_y: int
    """The vertical scroll distance."""
    type: Literal["scroll"]
    For a scroll action, this property is always set to `scroll`.
    """The x-coordinate where the scroll occurred."""
    """The y-coordinate where the scroll occurred."""
    """The keys being held while scrolling."""
class Type(BaseModel):
    """An action to type in text."""
    """The text to type."""
    type: Literal["type"]
    For a type action, this property is always set to `type`.
class Wait(BaseModel):
    """A wait action."""
    type: Literal["wait"]
    For a wait action, this property is always set to `wait`.
ComputerAction: TypeAlias = Annotated[
    Union[Click, DoubleClick, Drag, Keypress, Move, Screenshot, Scroll, Type, Wait], PropertyInfo(discriminator="type")
