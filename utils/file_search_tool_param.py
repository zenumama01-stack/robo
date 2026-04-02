__all__ = ["FileSearchToolParam", "FileSearch", "FileSearchRankingOptions"]
class FileSearchRankingOptions(TypedDict, total=False):
    score_threshold: Required[float]
    ranker: Literal["auto", "default_2024_08_21"]
class FileSearch(TypedDict, total=False):
    ranking_options: FileSearchRankingOptions
class FileSearchToolParam(TypedDict, total=False):
    type: Required[Literal["file_search"]]
    file_search: FileSearch
from ..shared_params.compound_filter import CompoundFilter
from ..shared_params.comparison_filter import ComparisonFilter
__all__ = ["FileSearchToolParam", "Filters", "RankingOptions", "RankingOptionsHybridSearch"]
class RankingOptionsHybridSearch(TypedDict, total=False):
    embedding_weight: Required[float]
    text_weight: Required[float]
    hybrid_search: RankingOptionsHybridSearch
    ranker: Literal["auto", "default-2024-11-15"]
    vector_store_ids: Required[SequenceNotStr[str]]
    filters: Optional[Filters]
