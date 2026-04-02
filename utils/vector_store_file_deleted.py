__all__ = ["VectorStoreFileDeleted"]
class VectorStoreFileDeleted(BaseModel):
    object: Literal["vector_store.file.deleted"]
