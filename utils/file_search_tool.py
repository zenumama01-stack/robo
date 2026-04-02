__all__ = ["FileSearchTool", "FileSearch", "FileSearchRankingOptions"]
class FileSearchRankingOptions(BaseModel):
    """The ranking options for the file search.
    If not specified, the file search tool will use the `auto` ranker and a score_threshold of 0.
    See the [file search tool documentation](https://platform.openai.com/docs/assistants/tools/file-search#customizing-file-search-settings) for more information.
    """The score threshold for the file search.
    All values must be a floating point number between 0 and 1.
    ranker: Optional[Literal["auto", "default_2024_08_21"]] = None
    """The ranker to use for the file search.
    If not specified will use the `auto` ranker.
class FileSearch(BaseModel):
    """Overrides for the file search tool."""
    max_num_results: Optional[int] = None
    """The maximum number of results the file search tool should output.
    The default is 20 for `gpt-4*` models and 5 for `gpt-3.5-turbo`. This number
    should be between 1 and 50 inclusive.
    Note that the file search tool may output fewer than `max_num_results` results.
    ranking_options: Optional[FileSearchRankingOptions] = None
    If not specified, the file search tool will use the `auto` ranker and a
    score_threshold of 0.
class FileSearchTool(BaseModel):
    type: Literal["file_search"]
    """The type of tool being defined: `file_search`"""
    file_search: Optional[FileSearch] = None
from ..shared.compound_filter import CompoundFilter
from ..shared.comparison_filter import ComparisonFilter
__all__ = ["FileSearchTool", "Filters", "RankingOptions", "RankingOptionsHybridSearch"]
Filters: TypeAlias = Union[ComparisonFilter, CompoundFilter, None]
class RankingOptionsHybridSearch(BaseModel):
    Weights that control how reciprocal rank fusion balances semantic embedding matches versus sparse keyword matches when hybrid search is enabled.
    embedding_weight: float
    """The weight of the embedding in the reciprocal ranking fusion."""
    text_weight: float
    """The weight of the text in the reciprocal ranking fusion."""
class RankingOptions(BaseModel):
    hybrid_search: Optional[RankingOptionsHybridSearch] = None
    Weights that control how reciprocal rank fusion balances semantic embedding
    matches versus sparse keyword matches when hybrid search is enabled.
    ranker: Optional[Literal["auto", "default-2024-11-15"]] = None
    """The ranker to use for the file search."""
    score_threshold: Optional[float] = None
    """The score threshold for the file search, a number between 0 and 1.
    Numbers closer to 1 will attempt to return only the most relevant results, but
    may return fewer results.
    """A tool that searches for relevant content from uploaded files.
    Learn more about the [file search tool](https://platform.openai.com/docs/guides/tools-file-search).
    """The type of the file search tool. Always `file_search`."""
    vector_store_ids: List[str]
    """The IDs of the vector stores to search."""
    filters: Optional[Filters] = None
    """A filter to apply."""
    ranking_options: Optional[RankingOptions] = None
