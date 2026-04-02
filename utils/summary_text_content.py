__all__ = ["SummaryTextContent"]
class SummaryTextContent(BaseModel):
    """A summary text from the model."""
    """A summary of the reasoning output from the model so far."""
    type: Literal["summary_text"]
    """The type of the object. Always `summary_text`."""
