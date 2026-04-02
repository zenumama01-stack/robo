__all__ = ["ResponseFormatText"]
class ResponseFormatText(BaseModel):
    """Default response format. Used to generate text responses."""
    """The type of response format being defined. Always `text`."""
class ResponseFormatText(TypedDict, total=False):
