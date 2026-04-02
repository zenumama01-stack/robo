__all__ = ["ContainerNetworkPolicyDisabled"]
class ContainerNetworkPolicyDisabled(BaseModel):
    type: Literal["disabled"]
    """Disable outbound network access. Always `disabled`."""
