__all__ = ["ChatKitWidgetItem"]
class ChatKitWidgetItem(BaseModel):
    """Thread item that renders a widget payload."""
    type: Literal["chatkit.widget"]
    """Type discriminator that is always `chatkit.widget`."""
    widget: str
    """Serialized widget payload rendered in the UI."""
