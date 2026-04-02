__all__ = ["ResponseOutputItemDoneEvent"]
class ResponseOutputItemDoneEvent(BaseModel):
    type: Literal["response.output_item.done"]
    """The event type, must be `response.output_item.done`."""
    """Returned when an Item is done streaming.
    Also emitted when a Response is
    interrupted, incomplete, or cancelled.
    """Emitted when an output item is marked done."""
    """The output item that was marked done."""
    """The index of the output item that was marked done."""
    """The type of the event. Always `response.output_item.done`."""
