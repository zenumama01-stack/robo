__all__ = ["SkillVersion"]
class SkillVersion(BaseModel):
    """Unique identifier for the skill version."""
    """Unix timestamp (seconds) for when the version was created."""
    """Description of the skill version."""
    """Name of the skill version."""
    object: Literal["skill.version"]
    """The object type, which is `skill.version`."""
    """Identifier of the skill for this version."""
    """Version number for this skill."""
