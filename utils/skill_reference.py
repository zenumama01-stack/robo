__all__ = ["SkillReference"]
class SkillReference(BaseModel):
    skill_id: str
    """The ID of the referenced skill."""
    type: Literal["skill_reference"]
    """References a skill created with the /v1/skills endpoint."""
    """Optional skill version. Use a positive integer or 'latest'. Omit for default."""
