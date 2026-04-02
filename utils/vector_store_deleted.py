__all__ = ["VectorStoreDeleted"]
class VectorStoreDeleted(BaseModel):
    object: Literal["vector_store.deleted"]
