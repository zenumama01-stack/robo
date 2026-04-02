from .inline_skill_source import InlineSkillSource
__all__ = ["InlineSkill"]
class InlineSkill(BaseModel):
    """The description of the skill."""
    """The name of the skill."""
    source: InlineSkillSource
    """Inline skill payload"""
    type: Literal["inline"]
    """Defines an inline skill for this request."""
