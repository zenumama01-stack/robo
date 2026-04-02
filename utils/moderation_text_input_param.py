__all__ = ["ModerationTextInputParam"]
class ModerationTextInputParam(TypedDict, total=False):
    """An object describing text to classify."""
    """A string of text to classify."""
    type: Required[Literal["text"]]
    """Always `text`."""
