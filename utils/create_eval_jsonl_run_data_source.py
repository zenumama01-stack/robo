__all__ = ["CreateEvalJSONLRunDataSource", "Source", "SourceFileContent", "SourceFileContentContent", "SourceFileID"]
Source: TypeAlias = Annotated[Union[SourceFileContent, SourceFileID], PropertyInfo(discriminator="type")]
class CreateEvalJSONLRunDataSource(BaseModel):
    A JsonlRunDataSource object with that specifies a JSONL file that matches the eval
    """Determines what populates the `item` namespace in the data source."""
    type: Literal["jsonl"]
    """The type of data source. Always `jsonl`."""
