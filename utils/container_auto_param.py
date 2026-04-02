from .inline_skill_param import InlineSkillParam
from .skill_reference_param import SkillReferenceParam
from .container_network_policy_disabled_param import ContainerNetworkPolicyDisabledParam
from .container_network_policy_allowlist_param import ContainerNetworkPolicyAllowlistParam
__all__ = ["ContainerAutoParam", "NetworkPolicy", "Skill"]
class ContainerAutoParam(TypedDict, total=False):
    type: Required[Literal["container_auto"]]
    memory_limit: Optional[Literal["1g", "4g", "16g", "64g"]]
