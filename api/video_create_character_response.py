__all__ = ["VideoCreateCharacterResponse"]
class VideoCreateCharacterResponse(BaseModel):
    id: Optional[str] = None
    """Identifier for the character creation cameo."""
    """Unix timestamp (in seconds) when the character was created."""
    name: Optional[str] = None
    """Display name for the character."""
