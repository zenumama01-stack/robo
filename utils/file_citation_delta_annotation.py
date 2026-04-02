__all__ = ["FileCitationDeltaAnnotation", "FileCitation"]
    file_id: Optional[str] = None
    quote: Optional[str] = None
    """The specific quote in the file."""
class FileCitationDeltaAnnotation(BaseModel):
    """The index of the annotation in the text content part."""
    end_index: Optional[int] = None
    file_citation: Optional[FileCitation] = None
    start_index: Optional[int] = None
