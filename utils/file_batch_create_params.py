from ..file_chunking_strategy_param import FileChunkingStrategyParam
__all__ = ["FileBatchCreateParams", "File"]
class FileBatchCreateParams(TypedDict, total=False):
    files: Iterable[File]
    A list of objects that each include a `file_id` plus optional `attributes` or
    A [File](https://platform.openai.com/docs/api-reference/files) ID that the
