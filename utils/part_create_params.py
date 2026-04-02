__all__ = ["PartCreateParams"]
class PartCreateParams(TypedDict, total=False):
    data: Required[FileTypes]
    """The chunk of bytes for this Part."""
