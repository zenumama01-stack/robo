from .skill_version import SkillVersion
__all__ = ["SkillVersionList"]
class SkillVersionList(BaseModel):
    data: List[SkillVersion]
