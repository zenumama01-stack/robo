from tests.utils import assert_matches_type
from openai.types import Batch
from openai.pagination import SyncCursorPage, AsyncCursorPage
class TestBatches:
    parametrize = pytest.mark.parametrize("client", [False, True], indirect=True, ids=["loose", "strict"])
    def test_method_create(self, client: OpenAI) -> None:
        batch = client.batches.create(
            completion_window="24h",
            endpoint="/v1/responses",
            input_file_id="string",
        assert_matches_type(Batch, batch, path=["response"])
    def test_method_create_with_all_params(self, client: OpenAI) -> None:
            metadata={"foo": "string"},
            output_expires_after={
                "anchor": "created_at",
                "seconds": 3600,
    def test_raw_response_create(self, client: OpenAI) -> None:
        response = client.batches.with_raw_response.create(
        assert response.is_closed is True
        assert response.http_request.headers.get("X-Stainless-Lang") == "python"
    def test_streaming_response_create(self, client: OpenAI) -> None:
        with client.batches.with_streaming_response.create(
            assert not response.is_closed
        assert cast(Any, response.is_closed) is True
    def test_method_retrieve(self, client: OpenAI) -> None:
        batch = client.batches.retrieve(
            "string",
    def test_raw_response_retrieve(self, client: OpenAI) -> None:
        response = client.batches.with_raw_response.retrieve(
    def test_streaming_response_retrieve(self, client: OpenAI) -> None:
        with client.batches.with_streaming_response.retrieve(
    def test_path_params_retrieve(self, client: OpenAI) -> None:
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `batch_id` but received ''"):
            client.batches.with_raw_response.retrieve(
                "",
    def test_method_list(self, client: OpenAI) -> None:
        batch = client.batches.list()
        assert_matches_type(SyncCursorPage[Batch], batch, path=["response"])
    def test_method_list_with_all_params(self, client: OpenAI) -> None:
        batch = client.batches.list(
            after="string",
            limit=0,
    def test_raw_response_list(self, client: OpenAI) -> None:
        response = client.batches.with_raw_response.list()
    def test_streaming_response_list(self, client: OpenAI) -> None:
        with client.batches.with_streaming_response.list() as response:
    def test_method_cancel(self, client: OpenAI) -> None:
        batch = client.batches.cancel(
    def test_raw_response_cancel(self, client: OpenAI) -> None:
        response = client.batches.with_raw_response.cancel(
    def test_streaming_response_cancel(self, client: OpenAI) -> None:
        with client.batches.with_streaming_response.cancel(
    def test_path_params_cancel(self, client: OpenAI) -> None:
            client.batches.with_raw_response.cancel(
class TestAsyncBatches:
    parametrize = pytest.mark.parametrize(
        "async_client", [False, True, {"http_client": "aiohttp"}], indirect=True, ids=["loose", "strict", "aiohttp"]
    async def test_method_create(self, async_client: AsyncOpenAI) -> None:
        batch = await async_client.batches.create(
    async def test_method_create_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_create(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.batches.with_raw_response.create(
    async def test_streaming_response_create(self, async_client: AsyncOpenAI) -> None:
        async with async_client.batches.with_streaming_response.create(
            batch = await response.parse()
    async def test_method_retrieve(self, async_client: AsyncOpenAI) -> None:
        batch = await async_client.batches.retrieve(
    async def test_raw_response_retrieve(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.batches.with_raw_response.retrieve(
    async def test_streaming_response_retrieve(self, async_client: AsyncOpenAI) -> None:
        async with async_client.batches.with_streaming_response.retrieve(
    async def test_path_params_retrieve(self, async_client: AsyncOpenAI) -> None:
            await async_client.batches.with_raw_response.retrieve(
    async def test_method_list(self, async_client: AsyncOpenAI) -> None:
        batch = await async_client.batches.list()
        assert_matches_type(AsyncCursorPage[Batch], batch, path=["response"])
    async def test_method_list_with_all_params(self, async_client: AsyncOpenAI) -> None:
        batch = await async_client.batches.list(
    async def test_raw_response_list(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.batches.with_raw_response.list()
    async def test_streaming_response_list(self, async_client: AsyncOpenAI) -> None:
        async with async_client.batches.with_streaming_response.list() as response:
    async def test_method_cancel(self, async_client: AsyncOpenAI) -> None:
        batch = await async_client.batches.cancel(
    async def test_raw_response_cancel(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.batches.with_raw_response.cancel(
    async def test_streaming_response_cancel(self, async_client: AsyncOpenAI) -> None:
        async with async_client.batches.with_streaming_response.cancel(
    async def test_path_params_cancel(self, async_client: AsyncOpenAI) -> None:
            await async_client.batches.with_raw_response.cancel(
