__all__ = ["StringCheckGraderParam"]
class StringCheckGraderParam(TypedDict, total=False):
    operation: Required[Literal["eq", "ne", "like", "ilike"]]
    reference: Required[str]
    type: Required[Literal["string_check"]]
