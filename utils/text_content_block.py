from .text import Text
__all__ = ["TextContentBlock"]
class TextContentBlock(BaseModel):
    """The text content that is part of a message."""
    text: Text
