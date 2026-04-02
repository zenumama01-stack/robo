__all__ = ["ResponseOutputItemAddedEvent"]
class ResponseOutputItemAddedEvent(BaseModel):
    """The index of the output item in the Response."""
    """The ID of the Response to which the item belongs."""
    type: Literal["response.output_item.added"]
    """The event type, must be `response.output_item.added`."""
    """Returned when a new Item is created during Response generation."""
    """Emitted when a new output item is added."""
    item: ResponseOutputItem
    """The output item that was added."""
    """The index of the output item that was added."""
    """The type of the event. Always `response.output_item.added`."""
