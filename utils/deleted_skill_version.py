__all__ = ["DeletedSkillVersion"]
class DeletedSkillVersion(BaseModel):
    object: Literal["skill.version.deleted"]
    """The deleted skill version."""
