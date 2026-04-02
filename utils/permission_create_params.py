from ...._types import SequenceNotStr
__all__ = ["PermissionCreateParams"]
class PermissionCreateParams(TypedDict, total=False):
    project_ids: Required[SequenceNotStr[str]]
    """The project identifiers to grant access to."""
