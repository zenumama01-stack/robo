__all__ = ["ResponseInputFile"]
class ResponseInputFile(BaseModel):
    """A file input to the model."""
    type: Literal["input_file"]
    """The type of the input item. Always `input_file`."""
    file_data: Optional[str] = None
    """The content of the file to be sent to the model."""
    """The ID of the file to be sent to the model."""
    file_url: Optional[str] = None
    """The URL of the file to be sent to the model."""
    """The name of the file to be sent to the model."""
