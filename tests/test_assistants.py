from openai.types.beta import (
    Assistant,
    AssistantDeleted,
class TestAssistants:
            assistant = client.beta.assistants.create(
                model="gpt-4o",
        assert_matches_type(Assistant, assistant, path=["response"])
                reasoning_effort="none",
                response_format="auto",
                tool_resources={
                    "code_interpreter": {"file_ids": ["string"]},
                    "file_search": {
                        "vector_store_ids": ["string"],
                        "vector_stores": [
                                "chunking_strategy": {"type": "auto"},
                                "file_ids": ["string"],
                                "metadata": {"foo": "string"},
                tools=[{"type": "code_interpreter"}],
            response = client.beta.assistants.with_raw_response.create(
        assistant = response.parse()
            with client.beta.assistants.with_streaming_response.create(
            assistant = client.beta.assistants.retrieve(
                "assistant_id",
            response = client.beta.assistants.with_raw_response.retrieve(
            with client.beta.assistants.with_streaming_response.retrieve(
            with pytest.raises(ValueError, match=r"Expected a non-empty value for `assistant_id` but received ''"):
                client.beta.assistants.with_raw_response.retrieve(
            assistant = client.beta.assistants.update(
                assistant_id="assistant_id",
                    "file_search": {"vector_store_ids": ["string"]},
            response = client.beta.assistants.with_raw_response.update(
            with client.beta.assistants.with_streaming_response.update(
                client.beta.assistants.with_raw_response.update(
                    assistant_id="",
            assistant = client.beta.assistants.list()
        assert_matches_type(SyncCursorPage[Assistant], assistant, path=["response"])
            assistant = client.beta.assistants.list(
            response = client.beta.assistants.with_raw_response.list()
            with client.beta.assistants.with_streaming_response.list() as response:
            assistant = client.beta.assistants.delete(
        assert_matches_type(AssistantDeleted, assistant, path=["response"])
            response = client.beta.assistants.with_raw_response.delete(
            with client.beta.assistants.with_streaming_response.delete(
                client.beta.assistants.with_raw_response.delete(
class TestAsyncAssistants:
            assistant = await async_client.beta.assistants.create(
            response = await async_client.beta.assistants.with_raw_response.create(
            async with async_client.beta.assistants.with_streaming_response.create(
                assistant = await response.parse()
            assistant = await async_client.beta.assistants.retrieve(
            response = await async_client.beta.assistants.with_raw_response.retrieve(
            async with async_client.beta.assistants.with_streaming_response.retrieve(
                await async_client.beta.assistants.with_raw_response.retrieve(
            assistant = await async_client.beta.assistants.update(
            response = await async_client.beta.assistants.with_raw_response.update(
            async with async_client.beta.assistants.with_streaming_response.update(
                await async_client.beta.assistants.with_raw_response.update(
            assistant = await async_client.beta.assistants.list()
        assert_matches_type(AsyncCursorPage[Assistant], assistant, path=["response"])
            assistant = await async_client.beta.assistants.list(
            response = await async_client.beta.assistants.with_raw_response.list()
            async with async_client.beta.assistants.with_streaming_response.list() as response:
            assistant = await async_client.beta.assistants.delete(
            response = await async_client.beta.assistants.with_raw_response.delete(
            async with async_client.beta.assistants.with_streaming_response.delete(
                await async_client.beta.assistants.with_raw_response.delete(
def test_create_and_run_poll_method_definition_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.beta.threads.create_and_run,  # pyright: ignore[reportDeprecated]
        checking_client.beta.threads.create_and_run_poll,
        exclude_params={"stream"},
def test_create_and_run_stream_method_definition_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.beta.threads.create_and_run_stream,
def test_run_stream_method_definition_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.beta.threads.runs.create,  # pyright: ignore[reportDeprecated]
        checking_client.beta.threads.runs.stream,  # pyright: ignore[reportDeprecated]
def test_create_and_poll_method_definition_in_sync(sync: bool, client: OpenAI, async_client: AsyncOpenAI) -> None:
        checking_client.beta.threads.runs.create_and_poll,  # pyright: ignore[reportDeprecated]
