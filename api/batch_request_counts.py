__all__ = ["BatchRequestCounts"]
class BatchRequestCounts(BaseModel):
    completed: int
    """Number of requests that have been completed successfully."""
    failed: int
    """Number of requests that have failed."""
    total: int
    """Total number of requests in the batch."""
