    Thread,
    ThreadDeleted,
from openai.types.beta.threads import Run
class TestThreads:
            thread = client.beta.threads.create()
        assert_matches_type(Thread, thread, path=["response"])
            thread = client.beta.threads.create(
                        "attachments": [
                                "file_id": "file_id",
                                "tools": [{"type": "code_interpreter"}],
            response = client.beta.threads.with_raw_response.create()
        thread = response.parse()
            with client.beta.threads.with_streaming_response.create() as response:
            thread = client.beta.threads.retrieve(
                "thread_id",
            response = client.beta.threads.with_raw_response.retrieve(
            with client.beta.threads.with_streaming_response.retrieve(
            with pytest.raises(ValueError, match=r"Expected a non-empty value for `thread_id` but received ''"):
                client.beta.threads.with_raw_response.retrieve(
            thread = client.beta.threads.update(
                thread_id="thread_id",
            response = client.beta.threads.with_raw_response.update(
            with client.beta.threads.with_streaming_response.update(
                client.beta.threads.with_raw_response.update(
                    thread_id="",
            thread = client.beta.threads.delete(
        assert_matches_type(ThreadDeleted, thread, path=["response"])
            response = client.beta.threads.with_raw_response.delete(
            with client.beta.threads.with_streaming_response.delete(
                client.beta.threads.with_raw_response.delete(
    def test_method_create_and_run_overload_1(self, client: OpenAI) -> None:
            thread = client.beta.threads.create_and_run(
        assert_matches_type(Run, thread, path=["response"])
    def test_method_create_and_run_with_all_params_overload_1(self, client: OpenAI) -> None:
                max_completion_tokens=256,
                max_prompt_tokens=256,
                thread={
                    "tool_resources": {
                truncation_strategy={
                    "type": "auto",
                    "last_messages": 1,
    def test_raw_response_create_and_run_overload_1(self, client: OpenAI) -> None:
            response = client.beta.threads.with_raw_response.create_and_run(
    def test_streaming_response_create_and_run_overload_1(self, client: OpenAI) -> None:
            with client.beta.threads.with_streaming_response.create_and_run(
    def test_method_create_and_run_overload_2(self, client: OpenAI) -> None:
            thread_stream = client.beta.threads.create_and_run(
        thread_stream.response.close()
    def test_method_create_and_run_with_all_params_overload_2(self, client: OpenAI) -> None:
    def test_raw_response_create_and_run_overload_2(self, client: OpenAI) -> None:
    def test_streaming_response_create_and_run_overload_2(self, client: OpenAI) -> None:
class TestAsyncThreads:
            thread = await async_client.beta.threads.create()
            thread = await async_client.beta.threads.create(
            response = await async_client.beta.threads.with_raw_response.create()
            async with async_client.beta.threads.with_streaming_response.create() as response:
                thread = await response.parse()
            thread = await async_client.beta.threads.retrieve(
            response = await async_client.beta.threads.with_raw_response.retrieve(
            async with async_client.beta.threads.with_streaming_response.retrieve(
                await async_client.beta.threads.with_raw_response.retrieve(
            thread = await async_client.beta.threads.update(
            response = await async_client.beta.threads.with_raw_response.update(
            async with async_client.beta.threads.with_streaming_response.update(
                await async_client.beta.threads.with_raw_response.update(
            thread = await async_client.beta.threads.delete(
            response = await async_client.beta.threads.with_raw_response.delete(
            async with async_client.beta.threads.with_streaming_response.delete(
                await async_client.beta.threads.with_raw_response.delete(
    async def test_method_create_and_run_overload_1(self, async_client: AsyncOpenAI) -> None:
            thread = await async_client.beta.threads.create_and_run(
    async def test_method_create_and_run_with_all_params_overload_1(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_create_and_run_overload_1(self, async_client: AsyncOpenAI) -> None:
            response = await async_client.beta.threads.with_raw_response.create_and_run(
    async def test_streaming_response_create_and_run_overload_1(self, async_client: AsyncOpenAI) -> None:
            async with async_client.beta.threads.with_streaming_response.create_and_run(
    async def test_method_create_and_run_overload_2(self, async_client: AsyncOpenAI) -> None:
            thread_stream = await async_client.beta.threads.create_and_run(
        await thread_stream.response.aclose()
    async def test_method_create_and_run_with_all_params_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_create_and_run_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_streaming_response_create_and_run_overload_2(self, async_client: AsyncOpenAI) -> None:
from openai.types.beta.chatkit import ChatKitThread, ThreadDeleteResponse
from openai.types.beta.chatkit.chatkit_thread_item_list import Data
        thread = client.beta.chatkit.threads.retrieve(
            "cthr_123",
        assert_matches_type(ChatKitThread, thread, path=["response"])
        response = client.beta.chatkit.threads.with_raw_response.retrieve(
        with client.beta.chatkit.threads.with_streaming_response.retrieve(
            client.beta.chatkit.threads.with_raw_response.retrieve(
        thread = client.beta.chatkit.threads.list()
        assert_matches_type(SyncConversationCursorPage[ChatKitThread], thread, path=["response"])
        thread = client.beta.chatkit.threads.list(
        response = client.beta.chatkit.threads.with_raw_response.list()
        with client.beta.chatkit.threads.with_streaming_response.list() as response:
        thread = client.beta.chatkit.threads.delete(
        assert_matches_type(ThreadDeleteResponse, thread, path=["response"])
        response = client.beta.chatkit.threads.with_raw_response.delete(
        with client.beta.chatkit.threads.with_streaming_response.delete(
            client.beta.chatkit.threads.with_raw_response.delete(
    def test_method_list_items(self, client: OpenAI) -> None:
        thread = client.beta.chatkit.threads.list_items(
            thread_id="cthr_123",
        assert_matches_type(SyncConversationCursorPage[Data], thread, path=["response"])
    def test_method_list_items_with_all_params(self, client: OpenAI) -> None:
    def test_raw_response_list_items(self, client: OpenAI) -> None:
        response = client.beta.chatkit.threads.with_raw_response.list_items(
    def test_streaming_response_list_items(self, client: OpenAI) -> None:
        with client.beta.chatkit.threads.with_streaming_response.list_items(
    def test_path_params_list_items(self, client: OpenAI) -> None:
            client.beta.chatkit.threads.with_raw_response.list_items(
        thread = await async_client.beta.chatkit.threads.retrieve(
        response = await async_client.beta.chatkit.threads.with_raw_response.retrieve(
        async with async_client.beta.chatkit.threads.with_streaming_response.retrieve(
            await async_client.beta.chatkit.threads.with_raw_response.retrieve(
        thread = await async_client.beta.chatkit.threads.list()
        assert_matches_type(AsyncConversationCursorPage[ChatKitThread], thread, path=["response"])
        thread = await async_client.beta.chatkit.threads.list(
        response = await async_client.beta.chatkit.threads.with_raw_response.list()
        async with async_client.beta.chatkit.threads.with_streaming_response.list() as response:
        thread = await async_client.beta.chatkit.threads.delete(
        response = await async_client.beta.chatkit.threads.with_raw_response.delete(
        async with async_client.beta.chatkit.threads.with_streaming_response.delete(
            await async_client.beta.chatkit.threads.with_raw_response.delete(
    async def test_method_list_items(self, async_client: AsyncOpenAI) -> None:
        thread = await async_client.beta.chatkit.threads.list_items(
        assert_matches_type(AsyncConversationCursorPage[Data], thread, path=["response"])
    async def test_method_list_items_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_list_items(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.beta.chatkit.threads.with_raw_response.list_items(
    async def test_streaming_response_list_items(self, async_client: AsyncOpenAI) -> None:
        async with async_client.beta.chatkit.threads.with_streaming_response.list_items(
    async def test_path_params_list_items(self, async_client: AsyncOpenAI) -> None:
            await async_client.beta.chatkit.threads.with_raw_response.list_items(
