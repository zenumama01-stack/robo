__all__ = ["ContainerNetworkPolicyDomainSecretParam"]
class ContainerNetworkPolicyDomainSecretParam(TypedDict, total=False):
    domain: Required[str]
    value: Required[str]
