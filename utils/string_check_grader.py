__all__ = ["StringCheckGrader"]
class StringCheckGrader(BaseModel):
    A StringCheckGrader object that performs a string comparison between input and reference using a specified operation.
    """The input text. This may include template strings."""
    operation: Literal["eq", "ne", "like", "ilike"]
    """The string check operation to perform. One of `eq`, `ne`, `like`, or `ilike`."""
    reference: str
    """The reference text. This may include template strings."""
    type: Literal["string_check"]
    """The object type, which is always `string_check`."""
