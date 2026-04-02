from openai.types.vector_stores import (
    VectorStoreFile,
    VectorStoreFileBatch,
class TestFileBatches:
        file_batch = client.vector_stores.file_batches.create(
        assert_matches_type(VectorStoreFileBatch, file_batch, path=["response"])
            attributes={"foo": "string"},
            files=[
                    "attributes": {"foo": "string"},
        response = client.vector_stores.file_batches.with_raw_response.create(
        file_batch = response.parse()
        with client.vector_stores.file_batches.with_streaming_response.create(
            client.vector_stores.file_batches.with_raw_response.create(
        file_batch = client.vector_stores.file_batches.retrieve(
            batch_id="vsfb_abc123",
        response = client.vector_stores.file_batches.with_raw_response.retrieve(
        with client.vector_stores.file_batches.with_streaming_response.retrieve(
            client.vector_stores.file_batches.with_raw_response.retrieve(
                batch_id="",
        file_batch = client.vector_stores.file_batches.cancel(
            batch_id="batch_id",
        response = client.vector_stores.file_batches.with_raw_response.cancel(
        with client.vector_stores.file_batches.with_streaming_response.cancel(
            client.vector_stores.file_batches.with_raw_response.cancel(
    def test_method_list_files(self, client: OpenAI) -> None:
        file_batch = client.vector_stores.file_batches.list_files(
        assert_matches_type(SyncCursorPage[VectorStoreFile], file_batch, path=["response"])
    def test_method_list_files_with_all_params(self, client: OpenAI) -> None:
            filter="in_progress",
    def test_raw_response_list_files(self, client: OpenAI) -> None:
        response = client.vector_stores.file_batches.with_raw_response.list_files(
    def test_streaming_response_list_files(self, client: OpenAI) -> None:
        with client.vector_stores.file_batches.with_streaming_response.list_files(
    def test_path_params_list_files(self, client: OpenAI) -> None:
            client.vector_stores.file_batches.with_raw_response.list_files(
class TestAsyncFileBatches:
        file_batch = await async_client.vector_stores.file_batches.create(
        response = await async_client.vector_stores.file_batches.with_raw_response.create(
        async with async_client.vector_stores.file_batches.with_streaming_response.create(
            file_batch = await response.parse()
            await async_client.vector_stores.file_batches.with_raw_response.create(
        file_batch = await async_client.vector_stores.file_batches.retrieve(
        response = await async_client.vector_stores.file_batches.with_raw_response.retrieve(
        async with async_client.vector_stores.file_batches.with_streaming_response.retrieve(
            await async_client.vector_stores.file_batches.with_raw_response.retrieve(
        file_batch = await async_client.vector_stores.file_batches.cancel(
        response = await async_client.vector_stores.file_batches.with_raw_response.cancel(
        async with async_client.vector_stores.file_batches.with_streaming_response.cancel(
            await async_client.vector_stores.file_batches.with_raw_response.cancel(
    async def test_method_list_files(self, async_client: AsyncOpenAI) -> None:
        file_batch = await async_client.vector_stores.file_batches.list_files(
        assert_matches_type(AsyncCursorPage[VectorStoreFile], file_batch, path=["response"])
    async def test_method_list_files_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_list_files(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.vector_stores.file_batches.with_raw_response.list_files(
    async def test_streaming_response_list_files(self, async_client: AsyncOpenAI) -> None:
        async with async_client.vector_stores.file_batches.with_streaming_response.list_files(
    async def test_path_params_list_files(self, async_client: AsyncOpenAI) -> None:
            await async_client.vector_stores.file_batches.with_raw_response.list_files(
    # ensure helpers do not drift from generated spec
        checking_client.vector_stores.file_batches.create,
        checking_client.vector_stores.file_batches.create_and_poll,
