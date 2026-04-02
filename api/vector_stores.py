from typing import Union, Optional
from ...types import (
    FileChunkingStrategyParam,
    vector_store_list_params,
    vector_store_create_params,
    vector_store_search_params,
    vector_store_update_params,
from ...types.vector_store import VectorStore
from ...types.vector_store_deleted import VectorStoreDeleted
from ...types.vector_store_search_response import VectorStoreSearchResponse
__all__ = ["VectorStores", "AsyncVectorStores"]
class VectorStores(SyncAPIResource):
    def file_batches(self) -> FileBatches:
        return FileBatches(self._client)
    def with_raw_response(self) -> VectorStoresWithRawResponse:
        return VectorStoresWithRawResponse(self)
    def with_streaming_response(self) -> VectorStoresWithStreamingResponse:
        return VectorStoresWithStreamingResponse(self)
        description: str | Omit = omit,
        expires_after: vector_store_create_params.ExpiresAfter | Omit = omit,
    ) -> VectorStore:
        Create a vector store.
          description: A description for the vector store. Can be used to describe the vector store's
              purpose.
          expires_after: The expiration policy for a vector store.
          name: The name of the vector store.
            "/vector_stores",
                vector_store_create_params.VectorStoreCreateParams,
            cast_to=VectorStore,
        Retrieves a vector store.
            path_template("/vector_stores/{vector_store_id}", vector_store_id=vector_store_id),
        expires_after: Optional[vector_store_update_params.ExpiresAfter] | Omit = omit,
        Modifies a vector store.
                vector_store_update_params.VectorStoreUpdateParams,
    ) -> SyncCursorPage[VectorStore]:
        """Returns a list of vector stores.
            page=SyncCursorPage[VectorStore],
                    vector_store_list_params.VectorStoreListParams,
            model=VectorStore,
    ) -> VectorStoreDeleted:
        Delete a vector store.
            cast_to=VectorStoreDeleted,
    def search(
        query: Union[str, SequenceNotStr[str]],
        filters: vector_store_search_params.Filters | Omit = omit,
        max_num_results: int | Omit = omit,
        ranking_options: vector_store_search_params.RankingOptions | Omit = omit,
        rewrite_query: bool | Omit = omit,
    ) -> SyncPage[VectorStoreSearchResponse]:
        Search a vector store for relevant chunks based on a query and file attributes
        filter.
          query: A query string for a search
          filters: A filter to apply based on file attributes.
          max_num_results: The maximum number of results to return. This number should be between 1 and 50
              inclusive.
          ranking_options: Ranking options for search.
          rewrite_query: Whether to rewrite the natural language query for vector search.
            path_template("/vector_stores/{vector_store_id}/search", vector_store_id=vector_store_id),
            page=SyncPage[VectorStoreSearchResponse],
                    "query": query,
                    "filters": filters,
                    "max_num_results": max_num_results,
                    "ranking_options": ranking_options,
                    "rewrite_query": rewrite_query,
                vector_store_search_params.VectorStoreSearchParams,
            model=VectorStoreSearchResponse,
class AsyncVectorStores(AsyncAPIResource):
    def file_batches(self) -> AsyncFileBatches:
        return AsyncFileBatches(self._client)
    def with_raw_response(self) -> AsyncVectorStoresWithRawResponse:
        return AsyncVectorStoresWithRawResponse(self)
    def with_streaming_response(self) -> AsyncVectorStoresWithStreamingResponse:
        return AsyncVectorStoresWithStreamingResponse(self)
    ) -> AsyncPaginator[VectorStore, AsyncCursorPage[VectorStore]]:
            page=AsyncCursorPage[VectorStore],
    ) -> AsyncPaginator[VectorStoreSearchResponse, AsyncPage[VectorStoreSearchResponse]]:
            page=AsyncPage[VectorStoreSearchResponse],
class VectorStoresWithRawResponse:
    def __init__(self, vector_stores: VectorStores) -> None:
        self._vector_stores = vector_stores
            vector_stores.create,
            vector_stores.retrieve,
            vector_stores.update,
            vector_stores.list,
            vector_stores.delete,
        self.search = _legacy_response.to_raw_response_wrapper(
            vector_stores.search,
        return FilesWithRawResponse(self._vector_stores.files)
    def file_batches(self) -> FileBatchesWithRawResponse:
        return FileBatchesWithRawResponse(self._vector_stores.file_batches)
class AsyncVectorStoresWithRawResponse:
    def __init__(self, vector_stores: AsyncVectorStores) -> None:
        self.search = _legacy_response.async_to_raw_response_wrapper(
        return AsyncFilesWithRawResponse(self._vector_stores.files)
    def file_batches(self) -> AsyncFileBatchesWithRawResponse:
        return AsyncFileBatchesWithRawResponse(self._vector_stores.file_batches)
class VectorStoresWithStreamingResponse:
        self.search = to_streamed_response_wrapper(
        return FilesWithStreamingResponse(self._vector_stores.files)
    def file_batches(self) -> FileBatchesWithStreamingResponse:
        return FileBatchesWithStreamingResponse(self._vector_stores.file_batches)
class AsyncVectorStoresWithStreamingResponse:
        self.search = async_to_streamed_response_wrapper(
        return AsyncFilesWithStreamingResponse(self._vector_stores.files)
    def file_batches(self) -> AsyncFileBatchesWithStreamingResponse:
        return AsyncFileBatchesWithStreamingResponse(self._vector_stores.file_batches)
            f"/vector_stores/{vector_store_id}",
            f"/vector_stores/{vector_store_id}/search",
from typing import Any, Dict, List, Literal, Optional, Tuple, Union
class SupportedVectorStoreIntegrations(str, Enum):
    """Supported vector store integrations."""
class LiteLLM_VectorStoreConfig(TypedDict, total=False):
    """Parameters for initializing a vector store on Litellm proxy config.yaml"""
    vector_store_name: str
class LiteLLM_ManagedVectorStore(TypedDict, total=False):
    """LiteLLM managed vector store object - this is is the object stored in the database"""
    vector_store_metadata: Optional[Union[Dict[str, Any], str]]
    # credential fields
    # litellm_params
    # access control fields
class LiteLLM_ManagedVectorStoreListResponse(TypedDict, total=False):
    """Response format for listing vector stores"""
    object: Literal["list"]  # Always "list"
    data: List[LiteLLM_ManagedVectorStore]
class VectorStoreUpdateRequest(BaseModel):
    custom_llm_provider: Optional[str] = None
    vector_store_name: Optional[str] = None
    vector_store_description: Optional[str] = None
    vector_store_metadata: Optional[Dict] = None
class VectorStoreDeleteRequest(BaseModel):
class VectorStoreInfoRequest(BaseModel):
class VectorStoreResultContent(TypedDict, total=False):
    """Content of a vector store result"""
    text: Optional[str]
class VectorStoreSearchResult(TypedDict, total=False):
    """Result of a vector store search"""
    score: Optional[float]
    content: Optional[List[VectorStoreResultContent]]
    attributes: Optional[Dict]
class VectorStoreSearchResponse(TypedDict, total=False):
    """Response after searching a vector store"""
    object: Literal[
        "vector_store.search_results.page"
    ]  # Always "vector_store.search_results.page"
    data: Optional[List[VectorStoreSearchResult]]
class VectorStoreSearchOptionalRequestParams(TypedDict, total=False):
    """TypedDict for Optional parameters supported by the vector store search API."""
    filters: Optional[Dict]
    max_num_results: Optional[int]
    ranking_options: Optional[Dict]
    rewrite_query: Optional[bool]
class VectorStoreSearchRequest(VectorStoreSearchOptionalRequestParams, total=False):
    """Request body for searching a vector store"""
    query: Union[str, List[str]]
# Vector Store Creation Types
class VectorStoreExpirationPolicy(TypedDict, total=False):
    """The expiration policy for a vector store"""
    anchor: Literal[
        "last_active_at"
    ]  # Anchor timestamp after which the expiration policy applies
    days: int  # Number of days after anchor time that the vector store will expire
class VectorStoreAutoChunkingStrategy(TypedDict, total=False):
    """Auto chunking strategy configuration"""
    type: Literal["auto"]  # Always "auto"
class VectorStoreStaticChunkingStrategyConfig(TypedDict, total=False):
    """Static chunking strategy configuration"""
    max_chunk_size_tokens: int  # Maximum number of tokens per chunk
    chunk_overlap_tokens: int  # Number of tokens to overlap between chunks
class VectorStoreStaticChunkingStrategy(TypedDict, total=False):
    """Static chunking strategy"""
    type: Literal["static"]  # Always "static"
    static: VectorStoreStaticChunkingStrategyConfig
class VectorStoreChunkingStrategy(TypedDict, total=False):
    """Union type for chunking strategies"""
    # This can be either auto or static
    type: Literal["auto", "static"]
    static: Optional[VectorStoreStaticChunkingStrategyConfig]
class VectorStoreFileCounts(TypedDict, total=False):
    """File counts for a vector store"""
class VectorStoreCreateOptionalRequestParams(TypedDict, total=False):
    """TypedDict for Optional parameters supported by the vector store create API."""
    name: Optional[str]  # Name of the vector store
    file_ids: Optional[List[str]]  # List of File IDs that the vector store should use
    expires_after: Optional[
        VectorStoreExpirationPolicy
    ]  # Expiration policy for the vector store
    chunking_strategy: Optional[
        VectorStoreChunkingStrategy
    ]  # Chunking strategy for the files
    metadata: Optional[Dict[str, str]]  # Set of key-value pairs for metadata
class VectorStoreCreateRequest(VectorStoreCreateOptionalRequestParams, total=False):
    """Request body for creating a vector store"""
    pass  # All fields are optional for vector store creation
class VectorStoreCreateResponse(TypedDict, total=False):
    """Response after creating a vector store"""
    id: str  # ID of the vector store
    object: Literal["vector_store"]  # Always "vector_store"
    created_at: int  # Unix timestamp of when the vector store was created
    bytes: int  # Size of the vector store in bytes
    file_counts: VectorStoreFileCounts  # File counts for the vector store
    status: Literal["expired", "in_progress", "completed"]  # Status of the vector store
    expires_after: Optional[VectorStoreExpirationPolicy]  # Expiration policy
    expires_at: Optional[int]  # Unix timestamp of when the vector store expires
    last_active_at: Optional[
    ]  # Unix timestamp of when the vector store was last active
    metadata: Optional[Dict[str, str]]  # Metadata associated with the vector store
class IndexCreateLiteLLMParams(BaseModel):
    vector_store_index: str
class IndexCreateRequest(BaseModel):
    litellm_params: IndexCreateLiteLLMParams
    index_info: Optional[Dict[str, Any]] = None
class BaseVectorStoreAuthCredentials(TypedDict, total=False):
    query_params: dict
class LiteLLM_ManagedVectorStoreIndex(BaseModel):
    """LiteLLM managed vector store index object - this is is the object stored in the database"""
class VectorStoreIndexType(str, Enum):
    """Type of vector store index"""
    READ = "read"
    WRITE = "write"
class VectorStoreIndexEndpoints(TypedDict):
    """Endpoints for vector store index"""
    read: List[
        Tuple[Literal["GET", "POST", "PUT", "DELETE", "PATCH"], str]
    ]  # endpoints for reading a vector store index
    write: List[
    ]  # endpoints for writing a vector store index
VECTOR_STORE_OPENAI_PARAMS = Literal[
    "max_num_results",
    "ranking_options",
    "rewrite_query",
class VectorStoreToolParams:
    """Parameters extracted from a file_search tool definition"""
    filters: Optional[Dict] = None
    ranking_options: Optional[Dict] = None
    def to_dict(self) -> Dict:
        """Convert to dict, excluding None values"""
                "filters": self.filters,
                "max_num_results": self.max_num_results,
                "ranking_options": self.ranking_options,
