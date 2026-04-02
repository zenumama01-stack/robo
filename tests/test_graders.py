from openai.types.fine_tuning.alpha import (
    GraderRunResponse,
    GraderValidateResponse,
class TestGraders:
    def test_method_run(self, client: OpenAI) -> None:
        grader = client.fine_tuning.alpha.graders.run(
            grader={
            model_sample="model_sample",
        assert_matches_type(GraderRunResponse, grader, path=["response"])
    def test_method_run_with_all_params(self, client: OpenAI) -> None:
            item={},
    def test_raw_response_run(self, client: OpenAI) -> None:
        response = client.fine_tuning.alpha.graders.with_raw_response.run(
        grader = response.parse()
    def test_streaming_response_run(self, client: OpenAI) -> None:
        with client.fine_tuning.alpha.graders.with_streaming_response.run(
    def test_method_validate(self, client: OpenAI) -> None:
        grader = client.fine_tuning.alpha.graders.validate(
        assert_matches_type(GraderValidateResponse, grader, path=["response"])
    def test_method_validate_with_all_params(self, client: OpenAI) -> None:
    def test_raw_response_validate(self, client: OpenAI) -> None:
        response = client.fine_tuning.alpha.graders.with_raw_response.validate(
    def test_streaming_response_validate(self, client: OpenAI) -> None:
        with client.fine_tuning.alpha.graders.with_streaming_response.validate(
class TestAsyncGraders:
    async def test_method_run(self, async_client: AsyncOpenAI) -> None:
        grader = await async_client.fine_tuning.alpha.graders.run(
    async def test_method_run_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_run(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.fine_tuning.alpha.graders.with_raw_response.run(
    async def test_streaming_response_run(self, async_client: AsyncOpenAI) -> None:
        async with async_client.fine_tuning.alpha.graders.with_streaming_response.run(
            grader = await response.parse()
    async def test_method_validate(self, async_client: AsyncOpenAI) -> None:
        grader = await async_client.fine_tuning.alpha.graders.validate(
    async def test_method_validate_with_all_params(self, async_client: AsyncOpenAI) -> None:
    async def test_raw_response_validate(self, async_client: AsyncOpenAI) -> None:
        response = await async_client.fine_tuning.alpha.graders.with_raw_response.validate(
    async def test_streaming_response_validate(self, async_client: AsyncOpenAI) -> None:
        async with async_client.fine_tuning.alpha.graders.with_streaming_response.validate(
