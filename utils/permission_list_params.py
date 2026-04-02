__all__ = ["PermissionListParams"]
class PermissionListParams(TypedDict, total=False):
    """Identifier for the last permission ID from the previous pagination request."""
    """Number of permissions to retrieve."""
    order: Literal["ascending", "descending"]
    """The order in which to retrieve permissions."""
    """The ID of the project to get permissions for."""
