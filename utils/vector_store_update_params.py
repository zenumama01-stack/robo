__all__ = ["VectorStoreUpdateParams", "ExpiresAfter"]
class VectorStoreUpdateParams(TypedDict, total=False):
    expires_after: Optional[ExpiresAfter]
    name: Optional[str]
