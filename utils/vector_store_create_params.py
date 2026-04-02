from .file_chunking_strategy_param import FileChunkingStrategyParam
__all__ = ["VectorStoreCreateParams", "ExpiresAfter"]
class VectorStoreCreateParams(TypedDict, total=False):
    chunking_strategy: FileChunkingStrategyParam
    """The chunking strategy used to chunk the file(s).
    If not set, will use the `auto` strategy. Only applicable if `file_ids` is
    non-empty.
    """A description for the vector store.
    Can be used to describe the vector store's purpose.
    A list of [File](https://platform.openai.com/docs/api-reference/files) IDs that
    days: Required[int]
