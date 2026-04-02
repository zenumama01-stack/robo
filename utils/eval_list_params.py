__all__ = ["EvalListParams"]
class EvalListParams(TypedDict, total=False):
    """Identifier for the last eval from the previous pagination request."""
    """Number of evals to retrieve."""
    """Sort order for evals by timestamp.
    Use `asc` for ascending order or `desc` for descending order.
    order_by: Literal["created_at", "updated_at"]
    """Evals can be ordered by creation time or last updated time.
    Use `created_at` for creation time or `updated_at` for last updated time.
