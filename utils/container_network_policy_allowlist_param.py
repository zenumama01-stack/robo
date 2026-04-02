from .container_network_policy_domain_secret_param import ContainerNetworkPolicyDomainSecretParam
__all__ = ["ContainerNetworkPolicyAllowlistParam"]
class ContainerNetworkPolicyAllowlistParam(TypedDict, total=False):
    allowed_domains: Required[SequenceNotStr[str]]
    type: Required[Literal["allowlist"]]
    domain_secrets: Iterable[ContainerNetworkPolicyDomainSecretParam]
