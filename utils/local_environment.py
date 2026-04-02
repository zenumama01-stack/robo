from .local_skill import LocalSkill
__all__ = ["LocalEnvironment"]
class LocalEnvironment(BaseModel):
    type: Literal["local"]
    """Use a local computer environment."""
    skills: Optional[List[LocalSkill]] = None
    """An optional list of skills."""
