__all__ = ["OtherFileChunkingStrategyObject"]
class OtherFileChunkingStrategyObject(BaseModel):
    """This is returned when the chunking strategy is unknown.
    Typically, this is because the file was indexed before the `chunking_strategy` concept was introduced in the API.
    type: Literal["other"]
    """Always `other`."""
