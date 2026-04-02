class TestContent:
    def test_method_retrieve(self, client: OpenAI, respx_mock: MockRouter) -> None:
        respx_mock.get("/containers/container_id/files/file_id/content").mock(
            return_value=httpx.Response(200, json={"foo": "bar"})
        content = client.containers.files.content.retrieve(
        assert isinstance(content, _legacy_response.HttpxBinaryResponseContent)
        assert content.json() == {"foo": "bar"}
    def test_raw_response_retrieve(self, client: OpenAI, respx_mock: MockRouter) -> None:
        response = client.containers.files.content.with_raw_response.retrieve(
        content = response.parse()
        assert_matches_type(_legacy_response.HttpxBinaryResponseContent, content, path=["response"])
    def test_streaming_response_retrieve(self, client: OpenAI, respx_mock: MockRouter) -> None:
        with client.containers.files.content.with_streaming_response.retrieve(
            assert_matches_type(bytes, content, path=["response"])
            client.containers.files.content.with_raw_response.retrieve(
class TestAsyncContent:
    async def test_method_retrieve(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        content = await async_client.containers.files.content.retrieve(
    async def test_raw_response_retrieve(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        response = await async_client.containers.files.content.with_raw_response.retrieve(
    async def test_streaming_response_retrieve(self, async_client: AsyncOpenAI, respx_mock: MockRouter) -> None:
        async with async_client.containers.files.content.with_streaming_response.retrieve(
            content = await response.parse()
            await async_client.containers.files.content.with_raw_response.retrieve(
        respx_mock.get("/skills/skill_123/content").mock(return_value=httpx.Response(200, json={"foo": "bar"}))
        content = client.skills.content.retrieve(
        response = client.skills.content.with_raw_response.retrieve(
        with client.skills.content.with_streaming_response.retrieve(
            client.skills.content.with_raw_response.retrieve(
        content = await async_client.skills.content.retrieve(
        response = await async_client.skills.content.with_raw_response.retrieve(
        async with async_client.skills.content.with_streaming_response.retrieve(
            await async_client.skills.content.with_raw_response.retrieve(
        respx_mock.get("/skills/skill_123/versions/version/content").mock(
        content = client.skills.versions.content.retrieve(
        response = client.skills.versions.content.with_raw_response.retrieve(
        with client.skills.versions.content.with_streaming_response.retrieve(
            client.skills.versions.content.with_raw_response.retrieve(
        content = await async_client.skills.versions.content.retrieve(
        response = await async_client.skills.versions.content.with_raw_response.retrieve(
        async with async_client.skills.versions.content.with_streaming_response.retrieve(
            await async_client.skills.versions.content.with_raw_response.retrieve(
