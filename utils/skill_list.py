from .skill import Skill
__all__ = ["SkillList"]
class SkillList(BaseModel):
    data: List[Skill]
    """A list of items"""
    first_id: Optional[str] = None
    """The ID of the first item in the list."""
    has_more: bool
    """Whether there are more items available."""
    """The ID of the last item in the list."""
    """The type of object returned, must be `list`."""
