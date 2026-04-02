__all__ = ["ChatKitAttachment"]
class ChatKitAttachment(BaseModel):
    """Attachment metadata included on thread items."""
    """Identifier for the attachment."""
    mime_type: str
    """MIME type of the attachment."""
    """Original display name for the attachment."""
    preview_url: Optional[str] = None
    """Preview URL for rendering the attachment inline."""
    type: Literal["image", "file"]
    """Attachment discriminator."""
