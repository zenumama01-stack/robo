__all__ = ["SkillUpdateParams"]
class SkillUpdateParams(TypedDict, total=False):
    default_version: Required[str]
    """The skill version number to set as default."""
