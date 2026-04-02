__all__ = ["FileCitationAnnotation", "FileCitation"]
class FileCitation(BaseModel):
    """The ID of the specific File the citation is from."""
class FileCitationAnnotation(BaseModel):
    A citation within the message that points to a specific quote from a specific File associated with the assistant or the message. Generated when the assistant uses the "file_search" tool to search files.
    end_index: int
    file_citation: FileCitation
    start_index: int
    """The text in the message content that needs to be replaced."""
    type: Literal["file_citation"]
    """Always `file_citation`."""
