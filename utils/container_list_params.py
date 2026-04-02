from typing_extensions import Literal, TypedDict
__all__ = ["ContainerListParams"]
class ContainerListParams(TypedDict, total=False):
    """Filter results by container name."""
    order: Literal["asc", "desc"]
    """Sort order by the `created_at` timestamp of the objects.
    `asc` for ascending order and `desc` for descending order.
