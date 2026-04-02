__all__ = ["ResponseOutputMessage", "Content"]
Content: TypeAlias = Annotated[Union[ResponseOutputText, ResponseOutputRefusal], PropertyInfo(discriminator="type")]
class ResponseOutputMessage(BaseModel):
    """An output message from the model."""
    """The unique ID of the output message."""
    """The content of the output message."""
    """The role of the output message. Always `assistant`."""
    """The type of the output message. Always `message`."""
