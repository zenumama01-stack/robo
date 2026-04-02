class TestRuns:
            run = client.beta.threads.runs.create(
        assert_matches_type(Run, run, path=["response"])
                include=["step_details.tool_calls[*].file_search.results[*].content"],
                additional_instructions="additional_instructions",
                additional_messages=[
            response = client.beta.threads.runs.with_raw_response.create(
            with client.beta.threads.runs.with_streaming_response.create(
    def test_path_params_create_overload_1(self, client: OpenAI) -> None:
                client.beta.threads.runs.with_raw_response.create(
            run_stream = client.beta.threads.runs.create(
        run_stream.response.close()
    def test_path_params_create_overload_2(self, client: OpenAI) -> None:
            run = client.beta.threads.runs.retrieve(
            response = client.beta.threads.runs.with_raw_response.retrieve(
            with client.beta.threads.runs.with_streaming_response.retrieve(
                client.beta.threads.runs.with_raw_response.retrieve(
            with pytest.raises(ValueError, match=r"Expected a non-empty value for `run_id` but received ''"):
                    run_id="",
            run = client.beta.threads.runs.update(
            response = client.beta.threads.runs.with_raw_response.update(
            with client.beta.threads.runs.with_streaming_response.update(
                client.beta.threads.runs.with_raw_response.update(
            run = client.beta.threads.runs.list(
        assert_matches_type(SyncCursorPage[Run], run, path=["response"])
            response = client.beta.threads.runs.with_raw_response.list(
            with client.beta.threads.runs.with_streaming_response.list(
                client.beta.threads.runs.with_raw_response.list(
            run = client.beta.threads.runs.cancel(
            response = client.beta.threads.runs.with_raw_response.cancel(
            with client.beta.threads.runs.with_streaming_response.cancel(
                client.beta.threads.runs.with_raw_response.cancel(
    def test_method_submit_tool_outputs_overload_1(self, client: OpenAI) -> None:
            run = client.beta.threads.runs.submit_tool_outputs(
                tool_outputs=[{}],
    def test_method_submit_tool_outputs_with_all_params_overload_1(self, client: OpenAI) -> None:
                tool_outputs=[
                        "output": "output",
                        "tool_call_id": "tool_call_id",
    def test_raw_response_submit_tool_outputs_overload_1(self, client: OpenAI) -> None:
            response = client.beta.threads.runs.with_raw_response.submit_tool_outputs(
    def test_streaming_response_submit_tool_outputs_overload_1(self, client: OpenAI) -> None:
            with client.beta.threads.runs.with_streaming_response.submit_tool_outputs(
    def test_path_params_submit_tool_outputs_overload_1(self, client: OpenAI) -> None:
                client.beta.threads.runs.with_raw_response.submit_tool_outputs(
    def test_method_submit_tool_outputs_overload_2(self, client: OpenAI) -> None:
            run_stream = client.beta.threads.runs.submit_tool_outputs(
    def test_raw_response_submit_tool_outputs_overload_2(self, client: OpenAI) -> None:
    def test_streaming_response_submit_tool_outputs_overload_2(self, client: OpenAI) -> None:
    def test_path_params_submit_tool_outputs_overload_2(self, client: OpenAI) -> None:
class TestAsyncRuns:
            run = await async_client.beta.threads.runs.create(
            response = await async_client.beta.threads.runs.with_raw_response.create(
            async with async_client.beta.threads.runs.with_streaming_response.create(
                run = await response.parse()
    async def test_path_params_create_overload_1(self, async_client: AsyncOpenAI) -> None:
                await async_client.beta.threads.runs.with_raw_response.create(
            run_stream = await async_client.beta.threads.runs.create(
        await run_stream.response.aclose()
    async def test_path_params_create_overload_2(self, async_client: AsyncOpenAI) -> None:
            run = await async_client.beta.threads.runs.retrieve(
            response = await async_client.beta.threads.runs.with_raw_response.retrieve(
            async with async_client.beta.threads.runs.with_streaming_response.retrieve(
                await async_client.beta.threads.runs.with_raw_response.retrieve(
            run = await async_client.beta.threads.runs.update(
            response = await async_client.beta.threads.runs.with_raw_response.update(
            async with async_client.beta.threads.runs.with_streaming_response.update(
                await async_client.beta.threads.runs.with_raw_response.update(
            run = await async_client.beta.threads.runs.list(
        assert_matches_type(AsyncCursorPage[Run], run, path=["response"])
            response = await async_client.beta.threads.runs.with_raw_response.list(
            async with async_client.beta.threads.runs.with_streaming_response.list(
                await async_client.beta.threads.runs.with_raw_response.list(
            run = await async_client.beta.threads.runs.cancel(
            response = await async_client.beta.threads.runs.with_raw_response.cancel(
            async with async_client.beta.threads.runs.with_streaming_response.cancel(
                await async_client.beta.threads.runs.with_raw_response.cancel(
    async def test_method_submit_tool_outputs_overload_1(self, async_client: AsyncOpenAI) -> None:
            run = await async_client.beta.threads.runs.submit_tool_outputs(
    async def test_method_submit_tool_outputs_with_all_params_overload_1(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_submit_tool_outputs_overload_1(self, async_client: AsyncOpenAI) -> None:
            response = await async_client.beta.threads.runs.with_raw_response.submit_tool_outputs(
    async def test_streaming_response_submit_tool_outputs_overload_1(self, async_client: AsyncOpenAI) -> None:
            async with async_client.beta.threads.runs.with_streaming_response.submit_tool_outputs(
    async def test_path_params_submit_tool_outputs_overload_1(self, async_client: AsyncOpenAI) -> None:
                await async_client.beta.threads.runs.with_raw_response.submit_tool_outputs(
    async def test_method_submit_tool_outputs_overload_2(self, async_client: AsyncOpenAI) -> None:
            run_stream = await async_client.beta.threads.runs.submit_tool_outputs(
    async def test_raw_response_submit_tool_outputs_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_streaming_response_submit_tool_outputs_overload_2(self, async_client: AsyncOpenAI) -> None:
    async def test_path_params_submit_tool_outputs_overload_2(self, async_client: AsyncOpenAI) -> None:
from openai.types.evals import (
    RunListResponse,
    RunCancelResponse,
    RunCreateResponse,
    RunDeleteResponse,
    RunRetrieveResponse,
        run = client.evals.runs.create(
            data_source={
                "source": {
                    "content": [{"item": {"foo": "bar"}}],
                    "type": "file_content",
                "type": "jsonl",
        assert_matches_type(RunCreateResponse, run, path=["response"])
                    "content": [
                            "item": {"foo": "bar"},
                            "sample": {"foo": "bar"},
        response = client.evals.runs.with_raw_response.create(
        with client.evals.runs.with_streaming_response.create(
            client.evals.runs.with_raw_response.create(
        run = client.evals.runs.retrieve(
        assert_matches_type(RunRetrieveResponse, run, path=["response"])
        response = client.evals.runs.with_raw_response.retrieve(
        with client.evals.runs.with_streaming_response.retrieve(
            client.evals.runs.with_raw_response.retrieve(
        run = client.evals.runs.list(
        assert_matches_type(SyncCursorPage[RunListResponse], run, path=["response"])
            status="queued",
        response = client.evals.runs.with_raw_response.list(
        with client.evals.runs.with_streaming_response.list(
            client.evals.runs.with_raw_response.list(
        run = client.evals.runs.delete(
        assert_matches_type(RunDeleteResponse, run, path=["response"])
        response = client.evals.runs.with_raw_response.delete(
        with client.evals.runs.with_streaming_response.delete(
            client.evals.runs.with_raw_response.delete(
        run = client.evals.runs.cancel(
        assert_matches_type(RunCancelResponse, run, path=["response"])
        response = client.evals.runs.with_raw_response.cancel(
        with client.evals.runs.with_streaming_response.cancel(
            client.evals.runs.with_raw_response.cancel(
        run = await async_client.evals.runs.create(
        response = await async_client.evals.runs.with_raw_response.create(
        async with async_client.evals.runs.with_streaming_response.create(
            await async_client.evals.runs.with_raw_response.create(
        run = await async_client.evals.runs.retrieve(
        response = await async_client.evals.runs.with_raw_response.retrieve(
        async with async_client.evals.runs.with_streaming_response.retrieve(
            await async_client.evals.runs.with_raw_response.retrieve(
        run = await async_client.evals.runs.list(
        assert_matches_type(AsyncCursorPage[RunListResponse], run, path=["response"])
        response = await async_client.evals.runs.with_raw_response.list(
        async with async_client.evals.runs.with_streaming_response.list(
            await async_client.evals.runs.with_raw_response.list(
        run = await async_client.evals.runs.delete(
        response = await async_client.evals.runs.with_raw_response.delete(
        async with async_client.evals.runs.with_streaming_response.delete(
            await async_client.evals.runs.with_raw_response.delete(
        run = await async_client.evals.runs.cancel(
        response = await async_client.evals.runs.with_raw_response.cancel(
        async with async_client.evals.runs.with_streaming_response.cancel(
            await async_client.evals.runs.with_raw_response.cancel(
