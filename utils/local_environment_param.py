from .local_skill_param import LocalSkillParam
__all__ = ["LocalEnvironmentParam"]
class LocalEnvironmentParam(TypedDict, total=False):
    type: Required[Literal["local"]]
    skills: Iterable[LocalSkillParam]
