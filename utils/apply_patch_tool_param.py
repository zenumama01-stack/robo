__all__ = ["ApplyPatchToolParam"]
class ApplyPatchToolParam(TypedDict, total=False):
    type: Required[Literal["apply_patch"]]
