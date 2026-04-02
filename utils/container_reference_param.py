__all__ = ["ContainerReferenceParam"]
class ContainerReferenceParam(TypedDict, total=False):
    container_id: Required[str]
    type: Required[Literal["container_reference"]]
