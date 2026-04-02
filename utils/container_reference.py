__all__ = ["ContainerReference"]
class ContainerReference(BaseModel):
    """The ID of the referenced container."""
    type: Literal["container_reference"]
    """References a container created with the /v1/containers endpoint"""
