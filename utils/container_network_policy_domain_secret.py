__all__ = ["ContainerNetworkPolicyDomainSecret"]
class ContainerNetworkPolicyDomainSecret(BaseModel):
    domain: str
    """The domain associated with the secret."""
    """The name of the secret to inject for the domain."""
    """The secret value to inject for the domain."""
