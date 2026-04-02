from openai.types.beta.threads.runs import RunStep
class TestSteps:
            step = client.beta.threads.runs.steps.retrieve(
                step_id="step_id",
        assert_matches_type(RunStep, step, path=["response"])
    def test_method_retrieve_with_all_params(self, client: OpenAI) -> None:
            response = client.beta.threads.runs.steps.with_raw_response.retrieve(
        step = response.parse()
            with client.beta.threads.runs.steps.with_streaming_response.retrieve(
                client.beta.threads.runs.steps.with_raw_response.retrieve(
            with pytest.raises(ValueError, match=r"Expected a non-empty value for `step_id` but received ''"):
                    step_id="",
            step = client.beta.threads.runs.steps.list(
        assert_matches_type(SyncCursorPage[RunStep], step, path=["response"])
            response = client.beta.threads.runs.steps.with_raw_response.list(
            with client.beta.threads.runs.steps.with_streaming_response.list(
                client.beta.threads.runs.steps.with_raw_response.list(
class TestAsyncSteps:
            step = await async_client.beta.threads.runs.steps.retrieve(
    async def test_method_retrieve_with_all_params(self, async_client: AsyncOpenAI) -> None:
            response = await async_client.beta.threads.runs.steps.with_raw_response.retrieve(
            async with async_client.beta.threads.runs.steps.with_streaming_response.retrieve(
                step = await response.parse()
                await async_client.beta.threads.runs.steps.with_raw_response.retrieve(
            step = await async_client.beta.threads.runs.steps.list(
        assert_matches_type(AsyncCursorPage[RunStep], step, path=["response"])
            response = await async_client.beta.threads.runs.steps.with_raw_response.list(
            async with async_client.beta.threads.runs.steps.with_streaming_response.list(
                await async_client.beta.threads.runs.steps.with_raw_response.list(
