__all__ = ["VersionCreateParams"]
class VersionCreateParams(TypedDict, total=False):
    default: bool
    """Whether to set this version as the default."""
