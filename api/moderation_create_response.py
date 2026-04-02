from .moderation import Moderation
__all__ = ["ModerationCreateResponse"]
class ModerationCreateResponse(BaseModel):
    """Represents if a given text input is potentially harmful."""
    """The unique identifier for the moderation request."""
    """The model used to generate the moderation results."""
    results: List[Moderation]
    """A list of moderation objects."""
