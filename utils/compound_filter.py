from .comparison_filter import ComparisonFilter
__all__ = ["CompoundFilter", "Filter"]
Filter: TypeAlias = Union[ComparisonFilter, object]
class CompoundFilter(BaseModel):
    """Combine multiple filters using `and` or `or`."""
    filters: List[Filter]
    """Array of filters to combine.
    Items can be `ComparisonFilter` or `CompoundFilter`.
    type: Literal["and", "or"]
    """Type of operation: `and` or `or`."""
class CompoundFilter(TypedDict, total=False):
    filters: Required[Iterable[Filter]]
    type: Required[Literal["and", "or"]]
