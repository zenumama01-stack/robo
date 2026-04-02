    Video,
    VideoDeleteResponse,
    VideoGetCharacterResponse,
    VideoCreateCharacterResponse,
from openai.pagination import SyncConversationCursorPage, AsyncConversationCursorPage
class TestVideos:
        video = client.videos.create(
            prompt="x",
        assert_matches_type(Video, video, path=["response"])
            input_reference=b"Example data",
            seconds="4",
            size="720x1280",
        response = client.videos.with_raw_response.create(
        with client.videos.with_streaming_response.create(
        video = client.videos.retrieve(
            "video_123",
        response = client.videos.with_raw_response.retrieve(
        with client.videos.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `video_id` but received ''"):
            client.videos.with_raw_response.retrieve(
        video = client.videos.list()
        assert_matches_type(SyncConversationCursorPage[Video], video, path=["response"])
        video = client.videos.list(
        response = client.videos.with_raw_response.list()
        with client.videos.with_streaming_response.list() as response:
        video = client.videos.delete(
        assert_matches_type(VideoDeleteResponse, video, path=["response"])
        response = client.videos.with_raw_response.delete(
        with client.videos.with_streaming_response.delete(
            client.videos.with_raw_response.delete(
    def test_method_create_character(self, client: OpenAI) -> None:
        video = client.videos.create_character(
            name="x",
            video=b"Example data",
        assert_matches_type(VideoCreateCharacterResponse, video, path=["response"])
    def test_raw_response_create_character(self, client: OpenAI) -> None:
        response = client.videos.with_raw_response.create_character(
    def test_streaming_response_create_character(self, client: OpenAI) -> None:
        with client.videos.with_streaming_response.create_character(
    def test_method_download_content(self, client: OpenAI, respx_mock: MockRouter) -> None:
        respx_mock.get("/videos/video_123/content").mock(return_value=httpx.Response(200, json={"foo": "bar"}))
        video = client.videos.download_content(
            video_id="video_123",
        assert isinstance(video, _legacy_response.HttpxBinaryResponseContent)
        assert video.json() == {"foo": "bar"}
    def test_method_download_content_with_all_params(self, client: OpenAI, respx_mock: MockRouter) -> None:
            variant="video",
    def test_raw_response_download_content(self, client: OpenAI, respx_mock: MockRouter) -> None:
        response = client.videos.with_raw_response.download_content(
        assert_matches_type(_legacy_response.HttpxBinaryResponseContent, video, path=["response"])
    def test_streaming_response_download_content(self, client: OpenAI, respx_mock: MockRouter) -> None:
        with client.videos.with_streaming_response.download_content(
            assert_matches_type(bytes, video, path=["response"])
    def test_path_params_download_content(self, client: OpenAI) -> None:
            client.videos.with_raw_response.download_content(
                video_id="",
    def test_method_edit(self, client: OpenAI) -> None:
        video = client.videos.edit(
    def test_raw_response_edit(self, client: OpenAI) -> None:
        response = client.videos.with_raw_response.edit(
    def test_streaming_response_edit(self, client: OpenAI) -> None:
        with client.videos.with_streaming_response.edit(
    def test_method_extend(self, client: OpenAI) -> None:
        video = client.videos.extend(
    def test_raw_response_extend(self, client: OpenAI) -> None:
        response = client.videos.with_raw_response.extend(
    def test_streaming_response_extend(self, client: OpenAI) -> None:
        with client.videos.with_streaming_response.extend(
    def test_method_get_character(self, client: OpenAI) -> None:
        video = client.videos.get_character(
            "char_123",
        assert_matches_type(VideoGetCharacterResponse, video, path=["response"])
    def test_raw_response_get_character(self, client: OpenAI) -> None:
        response = client.videos.with_raw_response.get_character(
    def test_streaming_response_get_character(self, client: OpenAI) -> None:
        with client.videos.with_streaming_response.get_character(
    def test_path_params_get_character(self, client: OpenAI) -> None:
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `character_id` but received ''"):
            client.videos.with_raw_response.get_character(
    def test_method_remix(self, client: OpenAI) -> None:
        video = client.videos.remix(
    def test_raw_response_remix(self, client: OpenAI) -> None:
        response = client.videos.with_raw_response.remix(
    def test_streaming_response_remix(self, client: OpenAI) -> None:
        with client.videos.with_streaming_response.remix(
    def test_path_params_remix(self, client: OpenAI) -> None:
            client.videos.with_raw_response.remix(
class TestAsyncVideos:
        video = await async_client.videos.create(
        response = await async_client.videos.with_raw_response.create(
        async with async_client.videos.with_streaming_response.create(
            video = await response.parse()
        video = await async_client.videos.retrieve(
        response = await async_client.videos.with_raw_response.retrieve(
        async with async_client.videos.with_streaming_response.retrieve(
            await async_client.videos.with_raw_response.retrieve(
        video = await async_client.videos.list()
        assert_matches_type(AsyncConversationCursorPage[Video], video, path=["response"])
        video = await async_client.videos.list(
        response = await async_client.videos.with_raw_response.list()
        async with async_client.videos.with_streaming_response.list() as response:
        video = await async_client.videos.delete(
        response = await async_client.videos.with_raw_response.delete(
        async with async_client.videos.with_streaming_response.delete(
            await async_client.videos.with_raw_response.delete(
    async def test_method_create_character(self, async_client: AsyncOpenAI) -> None:
        video = await async_client.videos.create_character(
    async def test_raw_response_create_character(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.videos.with_raw_response.create_character(
    async def test_streaming_response_create_character(self, async_client: AsyncOpenAI) -> None:
        async with async_client.videos.with_streaming_response.create_character(
    async def test_method_download_content(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        video = await async_client.videos.download_content(
    async def test_method_download_content_with_all_params(
        self, async_client: AsyncOpenAI, respx_mock: MockRouter
    async def test_raw_response_download_content(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        response = await async_client.videos.with_raw_response.download_content(
    async def test_streaming_response_download_content(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        async with async_client.videos.with_streaming_response.download_content(
    async def test_path_params_download_content(self, async_client: AsyncOpenAI) -> None:
            await async_client.videos.with_raw_response.download_content(
    async def test_method_edit(self, async_client: AsyncOpenAI) -> None:
        video = await async_client.videos.edit(
    async def test_raw_response_edit(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.videos.with_raw_response.edit(
    async def test_streaming_response_edit(self, async_client: AsyncOpenAI) -> None:
        async with async_client.videos.with_streaming_response.edit(
    async def test_method_extend(self, async_client: AsyncOpenAI) -> None:
        video = await async_client.videos.extend(
    async def test_raw_response_extend(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.videos.with_raw_response.extend(
    async def test_streaming_response_extend(self, async_client: AsyncOpenAI) -> None:
        async with async_client.videos.with_streaming_response.extend(
    async def test_method_get_character(self, async_client: AsyncOpenAI) -> None:
        video = await async_client.videos.get_character(
    async def test_raw_response_get_character(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.videos.with_raw_response.get_character(
    async def test_streaming_response_get_character(self, async_client: AsyncOpenAI) -> None:
        async with async_client.videos.with_streaming_response.get_character(
    async def test_path_params_get_character(self, async_client: AsyncOpenAI) -> None:
            await async_client.videos.with_raw_response.get_character(
    async def test_method_remix(self, async_client: AsyncOpenAI) -> None:
        video = await async_client.videos.remix(
    async def test_raw_response_remix(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.videos.with_raw_response.remix(
    async def test_streaming_response_remix(self, async_client: AsyncOpenAI) -> None:
        async with async_client.videos.with_streaming_response.remix(
    async def test_path_params_remix(self, async_client: AsyncOpenAI) -> None:
            await async_client.videos.with_raw_response.remix(
def test_create_and_poll_method_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.videos.create,
        checking_client.videos.create_and_poll,
        exclude_params={"extra_headers", "extra_query", "extra_body", "timeout"},
