__all__ = ["Skill"]
class Skill(BaseModel):
    """Unique identifier for the skill."""
    """Unix timestamp (seconds) for when the skill was created."""
    default_version: str
    """Default version for the skill."""
    description: str
    """Description of the skill."""
    latest_version: str
    """Latest version for the skill."""
    """Name of the skill."""
    object: Literal["skill"]
    """The object type, which is `skill`."""
