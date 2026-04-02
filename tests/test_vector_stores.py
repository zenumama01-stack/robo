    VectorStore,
    VectorStoreDeleted,
    VectorStoreSearchResponse,
from openai.pagination import SyncPage, AsyncPage, SyncCursorPage, AsyncCursorPage
class TestVectorStores:
        vector_store = client.vector_stores.create()
        assert_matches_type(VectorStore, vector_store, path=["response"])
        vector_store = client.vector_stores.create(
            chunking_strategy={"type": "auto"},
            description="description",
                "days": 1,
        response = client.vector_stores.with_raw_response.create()
        vector_store = response.parse()
        with client.vector_stores.with_streaming_response.create() as response:
        vector_store = client.vector_stores.retrieve(
            "vector_store_id",
        response = client.vector_stores.with_raw_response.retrieve(
        with client.vector_stores.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `vector_store_id` but received ''"):
            client.vector_stores.with_raw_response.retrieve(
        vector_store = client.vector_stores.update(
            vector_store_id="vector_store_id",
        response = client.vector_stores.with_raw_response.update(
        with client.vector_stores.with_streaming_response.update(
            client.vector_stores.with_raw_response.update(
                vector_store_id="",
        vector_store = client.vector_stores.list()
        assert_matches_type(SyncCursorPage[VectorStore], vector_store, path=["response"])
        vector_store = client.vector_stores.list(
            before="before",
        response = client.vector_stores.with_raw_response.list()
        with client.vector_stores.with_streaming_response.list() as response:
        vector_store = client.vector_stores.delete(
        assert_matches_type(VectorStoreDeleted, vector_store, path=["response"])
        response = client.vector_stores.with_raw_response.delete(
        with client.vector_stores.with_streaming_response.delete(
            client.vector_stores.with_raw_response.delete(
    def test_method_search(self, client: OpenAI) -> None:
        vector_store = client.vector_stores.search(
            vector_store_id="vs_abc123",
            query="string",
        assert_matches_type(SyncPage[VectorStoreSearchResponse], vector_store, path=["response"])
    def test_method_search_with_all_params(self, client: OpenAI) -> None:
            filters={
                "key": "key",
                "type": "eq",
                "value": "string",
            max_num_results=1,
            ranking_options={
                "ranker": "none",
                "score_threshold": 0,
            rewrite_query=True,
    def test_raw_response_search(self, client: OpenAI) -> None:
        response = client.vector_stores.with_raw_response.search(
    def test_streaming_response_search(self, client: OpenAI) -> None:
        with client.vector_stores.with_streaming_response.search(
    def test_path_params_search(self, client: OpenAI) -> None:
            client.vector_stores.with_raw_response.search(
class TestAsyncVectorStores:
        vector_store = await async_client.vector_stores.create()
        vector_store = await async_client.vector_stores.create(
        response = await async_client.vector_stores.with_raw_response.create()
        async with async_client.vector_stores.with_streaming_response.create() as response:
            vector_store = await response.parse()
        vector_store = await async_client.vector_stores.retrieve(
        response = await async_client.vector_stores.with_raw_response.retrieve(
        async with async_client.vector_stores.with_streaming_response.retrieve(
            await async_client.vector_stores.with_raw_response.retrieve(
        vector_store = await async_client.vector_stores.update(
        response = await async_client.vector_stores.with_raw_response.update(
        async with async_client.vector_stores.with_streaming_response.update(
            await async_client.vector_stores.with_raw_response.update(
        vector_store = await async_client.vector_stores.list()
        assert_matches_type(AsyncCursorPage[VectorStore], vector_store, path=["response"])
        vector_store = await async_client.vector_stores.list(
        response = await async_client.vector_stores.with_raw_response.list()
        async with async_client.vector_stores.with_streaming_response.list() as response:
        vector_store = await async_client.vector_stores.delete(
        response = await async_client.vector_stores.with_raw_response.delete(
        async with async_client.vector_stores.with_streaming_response.delete(
            await async_client.vector_stores.with_raw_response.delete(
    async def test_method_search(self, async_client: AsyncOpenAI) -> None:
        vector_store = await async_client.vector_stores.search(
        assert_matches_type(AsyncPage[VectorStoreSearchResponse], vector_store, path=["response"])
    async def test_method_search_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_search(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.vector_stores.with_raw_response.search(
    async def test_streaming_response_search(self, async_client: AsyncOpenAI) -> None:
        async with async_client.vector_stores.with_streaming_response.search(
    async def test_path_params_search(self, async_client: AsyncOpenAI) -> None:
            await async_client.vector_stores.with_raw_response.search(
