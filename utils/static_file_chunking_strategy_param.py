__all__ = ["StaticFileChunkingStrategyParam"]
class StaticFileChunkingStrategyParam(TypedDict, total=False):
    chunk_overlap_tokens: Required[int]
    max_chunk_size_tokens: Required[int]
