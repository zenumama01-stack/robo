from openai.types import (
    ContainerListResponse,
    ContainerCreateResponse,
    ContainerRetrieveResponse,
class TestContainers:
        container = client.containers.create(
            name="name",
        assert_matches_type(ContainerCreateResponse, container, path=["response"])
            expires_after={
                "anchor": "last_active_at",
                "minutes": 0,
            file_ids=["string"],
            memory_limit="1g",
            network_policy={"type": "disabled"},
            skills=[
                    "skill_id": "x",
                    "type": "skill_reference",
                    "version": "version",
        response = client.containers.with_raw_response.create(
        container = response.parse()
        with client.containers.with_streaming_response.create(
        container = client.containers.retrieve(
            "container_id",
        assert_matches_type(ContainerRetrieveResponse, container, path=["response"])
        response = client.containers.with_raw_response.retrieve(
        with client.containers.with_streaming_response.retrieve(
        with pytest.raises(ValueError, match=r"Expected a non-empty value for `container_id` but received ''"):
            client.containers.with_raw_response.retrieve(
        container = client.containers.list()
        assert_matches_type(SyncCursorPage[ContainerListResponse], container, path=["response"])
        container = client.containers.list(
            after="after",
            order="asc",
        response = client.containers.with_raw_response.list()
        with client.containers.with_streaming_response.list() as response:
    def test_method_delete(self, client: OpenAI) -> None:
        container = client.containers.delete(
        assert container is None
    def test_raw_response_delete(self, client: OpenAI) -> None:
        response = client.containers.with_raw_response.delete(
    def test_streaming_response_delete(self, client: OpenAI) -> None:
        with client.containers.with_streaming_response.delete(
    def test_path_params_delete(self, client: OpenAI) -> None:
            client.containers.with_raw_response.delete(
class TestAsyncContainers:
        container = await async_client.containers.create(
        response = await async_client.containers.with_raw_response.create(
        async with async_client.containers.with_streaming_response.create(
            container = await response.parse()
        container = await async_client.containers.retrieve(
        response = await async_client.containers.with_raw_response.retrieve(
        async with async_client.containers.with_streaming_response.retrieve(
            await async_client.containers.with_raw_response.retrieve(
        container = await async_client.containers.list()
        assert_matches_type(AsyncCursorPage[ContainerListResponse], container, path=["response"])
        container = await async_client.containers.list(
        response = await async_client.containers.with_raw_response.list()
        async with async_client.containers.with_streaming_response.list() as response:
    async def test_method_delete(self, async_client: AsyncOpenAI) -> None:
        container = await async_client.containers.delete(
    async def test_raw_response_delete(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.containers.with_raw_response.delete(
    async def test_streaming_response_delete(self, async_client: AsyncOpenAI) -> None:
        async with async_client.containers.with_streaming_response.delete(
    async def test_path_params_delete(self, async_client: AsyncOpenAI) -> None:
            await async_client.containers.with_raw_response.delete(
