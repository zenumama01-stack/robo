__all__ = ["JobListParams"]
class JobListParams(TypedDict, total=False):
    """Identifier for the last job from the previous pagination request."""
    """Number of fine-tuning jobs to retrieve."""
    metadata: Optional[Dict[str, str]]
    """Optional metadata filter.
    To filter, use the syntax `metadata[k]=v`. Alternatively, set `metadata=null` to
    indicate no metadata.
