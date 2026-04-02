__all__ = ["SkillReferenceParam"]
class SkillReferenceParam(TypedDict, total=False):
    skill_id: Required[str]
    type: Required[Literal["skill_reference"]]
