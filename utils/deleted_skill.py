__all__ = ["DeletedSkill"]
class DeletedSkill(BaseModel):
    deleted: bool
    object: Literal["skill.deleted"]
