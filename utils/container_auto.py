from .inline_skill import InlineSkill
from .skill_reference import SkillReference
from .container_network_policy_disabled import ContainerNetworkPolicyDisabled
from .container_network_policy_allowlist import ContainerNetworkPolicyAllowlist
__all__ = ["ContainerAuto", "NetworkPolicy", "Skill"]
NetworkPolicy: TypeAlias = Annotated[
    Union[ContainerNetworkPolicyDisabled, ContainerNetworkPolicyAllowlist], PropertyInfo(discriminator="type")
Skill: TypeAlias = Annotated[Union[SkillReference, InlineSkill], PropertyInfo(discriminator="type")]
class ContainerAuto(BaseModel):
    type: Literal["container_auto"]
    """Automatically creates a container for this request"""
    """An optional list of uploaded files to make available to your code."""
    """The memory limit for the container."""
    skills: Optional[List[Skill]] = None
