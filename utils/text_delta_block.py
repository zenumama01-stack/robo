from .text_delta import TextDelta
__all__ = ["TextDeltaBlock"]
class TextDeltaBlock(BaseModel):
    text: Optional[TextDelta] = None
