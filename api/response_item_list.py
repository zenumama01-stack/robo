from .response_item import ResponseItem
__all__ = ["ResponseItemList"]
class ResponseItemList(BaseModel):
    """A list of Response items."""
    data: List[ResponseItem]
    """A list of items used to generate this response."""
