__all__ = ["ContainerNetworkPolicyDisabledParam"]
class ContainerNetworkPolicyDisabledParam(TypedDict, total=False):
    type: Required[Literal["disabled"]]
