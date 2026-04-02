from .inline_skill_source_param import InlineSkillSourceParam
__all__ = ["InlineSkillParam"]
class InlineSkillParam(TypedDict, total=False):
    description: Required[str]
    source: Required[InlineSkillSourceParam]
    type: Required[Literal["inline"]]
