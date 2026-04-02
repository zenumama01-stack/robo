__all__ = ["SkillCreateParams"]
class SkillCreateParams(TypedDict, total=False):
    files: Union[SequenceNotStr[FileTypes], FileTypes]
    """Skill files to upload (directory upload) or a single zip file."""
