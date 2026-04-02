from openai.types import Upload
class TestUploads:
        upload = client.uploads.create(
            bytes=0,
            filename="filename",
            mime_type="mime_type",
        assert_matches_type(Upload, upload, path=["response"])
        response = client.uploads.with_raw_response.create(
        upload = response.parse()
        with client.uploads.with_streaming_response.create(
        upload = client.uploads.cancel(
            "upload_abc123",
        response = client.uploads.with_raw_response.cancel(
        with client.uploads.with_streaming_response.cancel(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `upload_id` but received ''"):
            client.uploads.with_raw_response.cancel(
    def test_method_complete(self, client: OpenAI) -> None:
        upload = client.uploads.complete(
            upload_id="upload_abc123",
            part_ids=["string"],
    def test_method_complete_with_all_params(self, client: OpenAI) -> None:
            md5="md5",
    def test_raw_response_complete(self, client: OpenAI) -> None:
        response = client.uploads.with_raw_response.complete(
    def test_streaming_response_complete(self, client: OpenAI) -> None:
        with client.uploads.with_streaming_response.complete(
    def test_path_params_complete(self, client: OpenAI) -> None:
            client.uploads.with_raw_response.complete(
                upload_id="",
class TestAsyncUploads:
        upload = await async_client.uploads.create(
        response = await async_client.uploads.with_raw_response.create(
        async with async_client.uploads.with_streaming_response.create(
            upload = await response.parse()
        upload = await async_client.uploads.cancel(
        response = await async_client.uploads.with_raw_response.cancel(
        async with async_client.uploads.with_streaming_response.cancel(
            await async_client.uploads.with_raw_response.cancel(
    async def test_method_complete(self, async_client: AsyncOpenAI) -> None:
        upload = await async_client.uploads.complete(
    async def test_method_complete_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_complete(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.uploads.with_raw_response.complete(
    async def test_streaming_response_complete(self, async_client: AsyncOpenAI) -> None:
        async with async_client.uploads.with_streaming_response.complete(
    async def test_path_params_complete(self, async_client: AsyncOpenAI) -> None:
            await async_client.uploads.with_raw_response.complete(
