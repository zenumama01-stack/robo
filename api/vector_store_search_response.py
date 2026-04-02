__all__ = ["VectorStoreSearchResponse", "Content"]
class Content(BaseModel):
    """The text content returned from search."""
    type: Literal["text"]
    """The type of content."""
class VectorStoreSearchResponse(BaseModel):
    attributes: Optional[Dict[str, Union[str, float, bool]]] = None
    structured format, and querying for objects via API or the dashboard. Keys are
    strings with a maximum length of 64 characters. Values are strings with a
    maximum length of 512 characters, booleans, or numbers.
    content: List[Content]
    """Content chunks from the file."""
    """The ID of the vector store file."""
    """The name of the vector store file."""
    score: float
    """The similarity score for the result."""
